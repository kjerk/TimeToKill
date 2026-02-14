using Avalonia;
using Avalonia.Headless;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using TimeToKill.App.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace TimeToKill.App.Tests;

public class TestAppBuilder
{
	public static AppBuilder BuildAvaloniaApp()
	{
		IconProvider.Current.Register<FontAwesomeIconProvider>();
		
		return AppBuilder.Configure<TestApp>()
			.UseSkia()
			.UseHeadless(new AvaloniaHeadlessPlatformOptions {
				UseHeadlessDrawing = false
			});
	}
}
