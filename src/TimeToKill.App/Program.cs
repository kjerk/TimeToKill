using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommandLine;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using TimeToKill.App.Cli;
using TimeToKill.Tools;

namespace TimeToKill.App;

public sealed class Program
{
	public static CliOptions StartupOptions { get; private set; }
	public static SingleInstanceManager InstanceManager { get; private set; }

	[STAThread]
	public static void Main(string[] args)
	{
		// Attach to parent console so --help output is visible from terminals.
		// No-op when double-clicked (no parent console to attach to).
		Console.WriteLine("saklfjhnafskjfasn");
		NativeMethods.AttachToParentConsole();
		Console.WriteLine("dfohgjndfkhndfh");

		var parseResult = Parser.Default.ParseArguments<CliOptions>(args);

		parseResult.WithParsed(options => {
			InstanceManager = new SingleInstanceManager();

			if (!InstanceManager.TryAcquireInstance()) {
				// Another instance is running — send commands and exit
				if (options.HasCommands) {
					SendCommandsAndExit(options).GetAwaiter().GetResult();
				}
				InstanceManager.Dispose();
				InstanceManager = null;
				return;
			}

			// First instance — store options and start Avalonia
			StartupOptions = options;
			BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

			// Avalonia has exited — clean up
			InstanceManager.Dispose();
			InstanceManager = null;
		});
	}

	private static async Task SendCommandsAndExit(CliOptions options)
	{
		if (options.StartTimers?.Any() == true) {
			await SingleInstanceManager.SendCommandToRunningInstance(IpcCommand.StartTimer(options.StartTimers));
		}
	}

	public static AppBuilder BuildAvaloniaApp()
	{
		IconProvider.Current.Register<FontAwesomeIconProvider>();

		return AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
	}
}
