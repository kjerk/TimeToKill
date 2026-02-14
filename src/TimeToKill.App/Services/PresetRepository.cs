using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TimeToKill.Models;

namespace TimeToKill.App.Services;

public class PresetRepository
{
	private readonly string _filePath;
	private readonly JsonSerializerOptions _jsonOptions;
	private List<TimerPreset> _cachedPresets;

	public PresetRepository() : this(GetDefaultFilePath()) { }

	public PresetRepository(string filePath)
	{
		_filePath = filePath;
		_jsonOptions = new JsonSerializerOptions {
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		_cachedPresets = new List<TimerPreset>();
	}

	private static string GetDefaultFilePath()
	{
		var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var configFolder = Path.Combine(homeDir, ".config", "timetokill");
		Directory.CreateDirectory(configFolder);
		return Path.Combine(configFolder, "presets.json");
	}
	
	public List<TimerPreset> LoadPresets()
	{
		if (!File.Exists(_filePath)) {
			_cachedPresets = new List<TimerPreset>();
			return _cachedPresets;
		}
		
		try {
			var json = File.ReadAllText(_filePath);
			_cachedPresets = JsonSerializer.Deserialize<List<TimerPreset>>(json, _jsonOptions) ?? new List<TimerPreset>();
			return _cachedPresets;
		} catch {
			_cachedPresets = new List<TimerPreset>();
			return _cachedPresets;
		}
	}
	
	public void SavePresets(List<TimerPreset> presets)
	{
		_cachedPresets = presets;
		var json = JsonSerializer.Serialize(presets, _jsonOptions);
		File.WriteAllText(_filePath, json);
	}
	
	public void AddPreset(TimerPreset preset)
	{
		if (_cachedPresets.Count == 0) {
			LoadPresets();
		}
		
		_cachedPresets.Add(preset);
		SavePresets(_cachedPresets);
	}
	
	public void UpdatePreset(TimerPreset preset)
	{
		if (_cachedPresets.Count == 0) {
			LoadPresets();
		}
		
		var index = _cachedPresets.FindIndex(p => p.Id == preset.Id);
		if (index >= 0) {
			_cachedPresets[index] = preset;
			SavePresets(_cachedPresets);
		}
	}
	
	public void RemovePreset(Guid id)
	{
		if (_cachedPresets.Count == 0) {
			LoadPresets();
		}
		
		_cachedPresets.RemoveAll(p => p.Id == id);
		SavePresets(_cachedPresets);
	}
}
