using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
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
		var options = CliOptions.Parse(args);
		if (options == null)
			return;

		if (options.Help) {
			NativeMethods.AttachToParentConsole();
			CliOptions.PrintHelp();
			return;
		}

		InstanceManager = new SingleInstanceManager();

		if (!InstanceManager.TryAcquireInstance()) {
			if (options.HasCommands) {
				SendCommandsAndExit(options).GetAwaiter().GetResult();
			}
			InstanceManager.Dispose();
			InstanceManager = null;
			return;
		}

		StartupOptions = options;
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

		InstanceManager.Dispose();
		InstanceManager = null;
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
