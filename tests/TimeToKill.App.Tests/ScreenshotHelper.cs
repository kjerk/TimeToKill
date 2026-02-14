using Avalonia.Controls;
using Avalonia.Headless;

namespace TimeToKill.App.Tests;

public static class ScreenshotHelper
{
	private static readonly string ScreenshotDir = Path.Combine(AppContext.BaseDirectory, "Screenshots");
	
	// Captures the current rendered frame of a window and saves it as a PNG.
	// Returns the full path to the saved file.
	public static string Capture(Window window, string name)
	{
		Directory.CreateDirectory(ScreenshotDir);
		
		var frame = window.CaptureRenderedFrame();
		if (frame == null)
			throw new InvalidOperationException($"CaptureRenderedFrame returned null for '{name}'. Is the window shown and rendered?");
		
		var filePath = Path.Combine(ScreenshotDir, $"{name}.png");
		frame.Save(filePath);
		return filePath;
	}
}
