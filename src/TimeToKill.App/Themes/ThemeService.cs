using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Media;
using TimeToKill.Models;
using TimeToKill.Tools;

namespace TimeToKill.App.Themes;

public class ThemeService
{
	private const string BaseThemeKey = "base";
	
	private readonly Dictionary<string, Theme> _themes;
	private string _currentTheme;
	
	public IReadOnlyList<string> AvailableThemes { get; }
	public string CurrentTheme => _currentTheme;
	public Theme CurrentThemeData => _themes.GetValueOrDefault(_currentTheme);
	
	public event EventHandler<string> ThemeChanged;
	
	public ThemeService()
	{
		_themes = LoadThemes();
		AvailableThemes = _themes.Keys.Where(k => k != BaseThemeKey).OrderBy(k => k).ToList();
		_currentTheme = "default-dark";
	}
	
	private Dictionary<string, Theme> LoadThemes()
	{
		var tomlContent = LoadEmbeddedResource();
		if (string.IsNullOrEmpty(tomlContent)) {
			throw new InvalidOperationException("Failed to load themes.toml embedded resource");
		}
		
		var config = TomlTools.Parse<ThemeConfig>(tomlContent);
		
		if (!config.Themes.TryGetValue(BaseThemeKey, out var baseTheme)) {
			throw new InvalidOperationException("themes.toml must contain [themes.base]");
		}
		
		// Saturate all non-base themes with base defaults
		foreach (var kvp in config.Themes) {
			if (kvp.Key != BaseThemeKey) {
				kvp.Value.ApplyDefaults(baseTheme);
			}
		}
		
		return config.Themes;
	}
	
	private string LoadEmbeddedResource()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var names = assembly.GetManifestResourceNames();
		var match = names.FirstOrDefault(n => n.EndsWith("themes.toml", StringComparison.OrdinalIgnoreCase));
		
		if (match == null) return null;
		
		using var stream = assembly.GetManifestResourceStream(match);
		if (stream == null) return null;
		
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}
	
	public void ApplyTheme(string themeName)
	{
		if (!_themes.TryGetValue(themeName, out var theme)) {
			throw new ArgumentException($"Theme '{themeName}' not found");
		}
		
		var resources = Application.Current?.Resources;
		if (resources == null) return;
		
		foreach (var (name, value) in theme.GetColors()) {
			resources[name] = new SolidColorBrush(Color.Parse(value));
		}
		
		_currentTheme = themeName;
		ThemeChanged?.Invoke(this, themeName);
	}
	
	public Theme GetTheme(string themeName)
	{
		return _themes.TryGetValue(themeName, out var theme) ? theme : null;
	}
}
