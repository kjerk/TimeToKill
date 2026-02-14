namespace TimeToKill.App.Tests;

// Minimal App subclass for headless testing.
// Inherits App's AXAML styles (loaded via AvaloniaXamlLoader in Initialize),
// but skips all service wiring, tray icon setup, and window creation.
public class TestApp : App
{
	public override void OnFrameworkInitializationCompleted()
	{
		// Intentionally empty - tests wire their own services and windows.
	}
}
