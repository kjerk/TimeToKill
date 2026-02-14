using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TimeToKill.App.Cli;

public class SingleInstanceManager : IDisposable
{
	private const string MutexName = "TimeToKill_SingleInstance";
	private const string PipeName = "TimeToKill_Command";
	private const int PipeConnectionTimeoutMs = 3000;

	private Mutex _mutex;
	private bool _isFirstInstance;
	private CancellationTokenSource _pipeCancellation;
	private Task _pipeServerTask;

	public event EventHandler<IpcCommand> CommandReceived;

	public bool IsFirstInstance => _isFirstInstance;

	public bool TryAcquireInstance()
	{
		_mutex = new Mutex(true, MutexName, out _isFirstInstance);
		return _isFirstInstance;
	}

	public void StartPipeServer()
	{
		if (!_isFirstInstance)
			throw new InvalidOperationException("Cannot start pipe server on secondary instance.");

		_pipeCancellation = new CancellationTokenSource();
		_pipeServerTask = Task.Run(() => PipeServerLoop(_pipeCancellation.Token));
	}

	private async Task PipeServerLoop(CancellationToken cancellation)
	{
		while (!cancellation.IsCancellationRequested) {
			try {
				await using var server = new NamedPipeServerStream(
					PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
				await server.WaitForConnectionAsync(cancellation);
				await HandleClientConnection(server);
			} catch (OperationCanceledException) {
				break;
			} catch {
				// Connection-level error — backoff and re-listen
				try { await Task.Delay(1000, cancellation); }
				catch (OperationCanceledException) { break; }
			}
		}
	}

	private async Task HandleClientConnection(NamedPipeServerStream server)
	{
		try {
			using var reader = new StreamReader(server, Encoding.UTF8);
			var json = await reader.ReadToEndAsync();
			var command = JsonSerializer.Deserialize<IpcCommand>(json);
			if (command != null) {
				CommandReceived?.Invoke(this, command);
			}
		} catch {
			// Malformed message — ignore
		}
	}

	public static async Task<bool> SendCommandToRunningInstance(IpcCommand command)
	{
		try {
			await using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
			await client.ConnectAsync(PipeConnectionTimeoutMs);
			var json = JsonSerializer.Serialize(command);
			await using var writer = new StreamWriter(client, Encoding.UTF8);
			await writer.WriteAsync(json);
			await writer.FlushAsync();
			return true;
		} catch {
			return false;
		}
	}

	public void Dispose()
	{
		_pipeCancellation?.Cancel();
		try { _pipeServerTask?.Wait(TimeSpan.FromSeconds(2)); } catch { }
		_pipeCancellation?.Dispose();

		if (_isFirstInstance) {
			try { _mutex?.ReleaseMutex(); } catch { }
		}
		_mutex?.Dispose();
	}
}
