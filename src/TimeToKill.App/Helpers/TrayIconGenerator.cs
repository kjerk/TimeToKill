using System;
using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace TimeToKill.App.Helpers;

public enum TrayStatus
{
	Idle, // Yellow
	Active, // Green
	Fired // Red?
}

// A helper class using SkiaSharp to generate tray icons dynamically (so a logo can have a status dot)
public static class TrayIconGenerator
{
	private const int IconSize = 32;
	private const int DotSize = 10;
	private const int DotMargin = 2;
	
	private static readonly SKColor GreenColor = new SKColor(46, 204, 113); // #2ECC71
	private static readonly SKColor YellowColor = new SKColor(241, 196, 15); // #F1C40F
	private static readonly SKColor RedColor = new SKColor(231, 76, 60); // #E74C3C
	private static readonly SKColor BaseIconColor = new SKColor(100, 100, 100);
	
	public static Bitmap GenerateIcon(TrayStatus status)
	{
		var info = new SKImageInfo(IconSize, IconSize, SKColorType.Bgra8888, SKAlphaType.Premul);
		using var surface = SKSurface.Create(info);
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.Transparent);
		
		// Draw base icon (simple clock/timer shape)
		DrawBaseIcon(canvas);
		
		// Draw status dot
		DrawStatusDot(canvas, status);
		
		// Convert to Avalonia Bitmap
		using var image = surface.Snapshot();
		using var data = image.Encode(SKEncodedImageFormat.Png, 100);
		using var stream = new MemoryStream(data.ToArray());
		return new Bitmap(stream);
	}
	
	// TODO: Replace this with a proper icon, works for now.
	private static void DrawBaseIcon(SKCanvas canvas)
	{
		var center = IconSize / 2f;
		var radius = (IconSize - DotSize - DotMargin) / 2f - 2;
		
		// Draw clock circle
		using var circlePaint = new SKPaint {
			Color = BaseIconColor,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 2.5f,
			IsAntialias = true
		};
		canvas.DrawCircle(center - 2, center, radius, circlePaint);
		
		// Draw clock hands (pointing to ~2:00 for visual interest)
		using var handPaint = new SKPaint {
			Color = BaseIconColor,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 2f,
			StrokeCap = SKStrokeCap.Round,
			IsAntialias = true
		};
		
		// Hour hand (short)
		var hourEndX = center - 2 + (float)Math.Cos(Math.PI * -0.25) * (radius * 0.4f);
		var hourEndY = center + (float)Math.Sin(Math.PI * -0.25) * (radius * 0.4f);
		canvas.DrawLine(center - 2, center, hourEndX, hourEndY, handPaint);
		
		// Minute hand (long)
		var minEndX = center - 2 + (float)Math.Cos(Math.PI * -0.5) * (radius * 0.65f);
		var minEndY = center + (float)Math.Sin(Math.PI * -0.5) * (radius * 0.65f);
		canvas.DrawLine(center - 2, center, minEndX, minEndY, handPaint);
	}
	
	private static void DrawStatusDot(SKCanvas canvas, TrayStatus status)
	{
		var dotColor = status switch {
			TrayStatus.Active => GreenColor,
			TrayStatus.Fired => RedColor,
			_ => YellowColor
		};
		
		var dotX = IconSize - DotSize / 2f - DotMargin;
		var dotY = DotSize / 2f + DotMargin;
		
		// Draw dot with slight shadow/border for visibility
		using var shadowPaint = new SKPaint {
			Color = SKColors.Black.WithAlpha(100),
			Style = SKPaintStyle.Fill,
			IsAntialias = true
		};
		canvas.DrawCircle(dotX, dotY + 1, DotSize / 2f, shadowPaint);
		
		using var dotPaint = new SKPaint {
			Color = dotColor,
			Style = SKPaintStyle.Fill,
			IsAntialias = true
		};
		canvas.DrawCircle(dotX, dotY, DotSize / 2f, dotPaint);
		
		// White highlight for 3D effect
		using var highlightPaint = new SKPaint {
			Color = SKColors.White.WithAlpha(80),
			Style = SKPaintStyle.Fill,
			IsAntialias = true
		};
		canvas.DrawCircle(dotX - 1, dotY - 1, DotSize / 4f, highlightPaint);
	}
}
