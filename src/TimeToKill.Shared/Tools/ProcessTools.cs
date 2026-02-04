using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TimeToKill.Tools;

public static class ProcessTools
{
	// Kill all processes matching the given name using taskkill.
	public static (bool Success, int Count, string Error) KillByName(string processName, bool force = false)
	{
		if (string.IsNullOrWhiteSpace(processName)) {
			return (false, 0, "Process name cannot be empty");
		}
		
		// Strip path and normalize process name - ensure it ends with .exe for taskkill
		var targetName = ProcessNameHelper.GetExeName(processName.Trim());
		if (!targetName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
			targetName += ".exe";
		}
		
		// Count processes before kill for reporting
		var countBefore = CountProcessesByName(targetName);
		if (countBefore == 0) {
			return (false, 0, $"No processes found matching '{targetName}'");
		}
		
		try {
			var cliArgs = force ? $"/F /IM \"{targetName}\"" : $"/IM \"{targetName}\"";
			
			var startInfo = new ProcessStartInfo {
				FileName = "taskkill",
				Arguments = cliArgs,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			};
			
			using var process = Process.Start(startInfo);
			if (process == null) {
				return (false, 0, "Failed to start taskkill process");
			}
			
			process.WaitForExit(10000); // 10 second timeout
			
			var exitCode = process.ExitCode;
			var stdout = process.StandardOutput.ReadToEnd();
			var stderr = process.StandardError.ReadToEnd();
			
			if (exitCode == 0) {
				// Count how many were actually terminated
				var countAfter = CountProcessesByName(targetName);
				var terminated = countBefore - countAfter;
				return (true, terminated > 0 ? terminated : countBefore, null);
			} else {
				var error = !string.IsNullOrWhiteSpace(stderr) ? stderr : stdout;
				return (false, 0, error.Trim());
			}
		} catch (Exception ex) {
			return (false, 0, ex.Message);
		}
	}
	
	// Count running processes matching the given name.
	public static int CountProcessesByName(string processName)
	{
		// Strip path and remove .exe extension for Process.GetProcessesByName
		var name = ProcessNameHelper.GetBaseNameWithoutExtension(processName.Trim());

		try {
			var processes = Process.GetProcessesByName(name);
			var count = processes.Length;
			foreach (var proc in processes) {
				proc.Dispose();
			}
			return count;
		} catch {
			return 0;
		}
	}
	
	public static bool IsProcessRunning(string processName)
	{
		return CountProcessesByName(processName) > 0;
	}
	
	public static (bool Success, int Count, string Error) SuspendByName(string processName)
	{
		var processes = GetProcessesByName(processName);
		if (processes.Length == 0) {
			return (false, 0, $"No processes found matching '{processName}'");
		}

		int suspended = 0;
		try {
			foreach (var proc in processes) {
				try {
					// Suspend via NtSuspendProcess or by suspending all threads
					foreach (ProcessThread thread in proc.Threads) {
						var threadHandle = OpenThread(0x0002, false, (uint)thread.Id); // THREAD_SUSPEND_RESUME
						if (threadHandle != IntPtr.Zero) {
							SuspendThread(threadHandle);
							CloseHandle(threadHandle);
						}
					}

					suspended++;
				} catch {
					// Continue with other processes
				}
			}
		} finally {
			foreach (var proc in processes) {
				proc.Dispose();
			}
		}

		return suspended > 0
			? (true, suspended, null)
			: (false, 0, "Failed to suspend any processes");
	}
	
	// Set all matching processes to below-normal priority.
	public static (bool Success, int Count, string Error) DemotePriorityByName(string processName)
	{
		var processes = GetProcessesByName(processName);
		if (processes.Length == 0) {
			return (false, 0, $"No processes found matching '{processName}'");
		}

		int demoted = 0;
		try {
			foreach (var proc in processes) {
				try {
					proc.PriorityClass = ProcessPriorityClass.BelowNormal;
					demoted++;
				} catch {
					// Continue with other processes
				}
			}
		} finally {
			foreach (var proc in processes) {
				proc.Dispose();
			}
		}

		return demoted > 0
			? (true, demoted, null)
			: (false, 0, "Failed to demote priority of any processes");
	}
	
	// Launch a process by path.
	public static (bool Success, string Error) LaunchProcess(string processPath)
	{
		if (string.IsNullOrWhiteSpace(processPath)) {
			return (false, "Process path cannot be empty");
		}

		try {
			var startInfo = new ProcessStartInfo {
				FileName = processPath,
				UseShellExecute = true
			};
			var proc = Process.Start(startInfo);
			return proc != null
				? (true, (string)null)
				: (false, "Process.Start returned null");
		} catch (Exception ex) {
			return (false, ex.Message);
		}
	}

	// Returns Process objects that MUST be disposed by the caller.
	private static Process[] GetProcessesByName(string processName)
	{
		// Strip path and remove .exe extension for Process.GetProcessesByName
		var name = ProcessNameHelper.GetBaseNameWithoutExtension(processName.Trim());

		try {
			return Process.GetProcessesByName(name);
		} catch {
			return Array.Empty<Process>();
		}
	}
	
	// P/Invoke functions for thread suspension
	[DllImport("kernel32.dll")]
	private static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
	
	[DllImport("kernel32.dll")]
	private static extern uint SuspendThread(IntPtr hThread);
	
	[DllImport("kernel32.dll")]
	private static extern bool CloseHandle(IntPtr hObject);
}
