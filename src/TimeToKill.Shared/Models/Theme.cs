using TimeToKill.Tools;
using Tomlyn;

namespace TimeToKill.Models;

public class ThemeConfig
{
	[TomlPropertyName("themes")]
	public Dictionary<string, Theme> Themes { get; set; } = new();
}

// A model object that holds the app's main color variables.
public class Theme
{
	// Backgrounds
	public string AppBackground { get; set; }
	public string SurfaceBackground { get; set; }
	public string CardBackground { get; set; }
	public string CardBackgroundHover { get; set; }

	// Borders
	public string BorderSubtle { get; set; }
	public string BorderMedium { get; set; }

	// Text
	public string TextPrimary { get; set; }
	public string TextSecondary { get; set; }
	public string TextTertiary { get; set; }
	public string TextInverse { get; set; }

	// Accent - Primary
	public string AccentPrimary { get; set; }
	public string AccentPrimaryHover { get; set; }

	// Accent - Success
	public string AccentSuccess { get; set; }
	public string AccentSuccessBackground { get; set; }

	// Accent - Warning
	public string AccentWarning { get; set; }

	// Accent - Danger
	public string AccentDanger { get; set; }
	public string AccentDangerHover { get; set; }
	public string AccentDangerBackground { get; set; }

	// Controls
	public string ButtonSecondaryBackground { get; set; }
	public string ButtonSecondaryBackgroundHover { get; set; }
	public string InputBackground { get; set; }
	public string InputBorder { get; set; }

	// Progress
	public string ProgressBackground { get; set; }
	public string ProgressForeground { get; set; }
	
	public void ApplyDefaults(Theme baseTheme)
	{
		if (baseTheme == null) return;
		ReflectTools.MergeStringProperties(baseTheme, this);
	}
	
	public IEnumerable<(string Name, string Value)> GetColors()
	{
		return ReflectTools.IteratePropertiesWithValues<Theme, string>(this, nonNullOnly: true);
	}
}
