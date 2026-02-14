using System;
using System.Collections.Generic;
using System.Linq;
using TimeToKill.Models;
using TimeToKill.Tools;

namespace TimeToKill.App.Cli;

public class IdentifierResolver
{
	private readonly List<TimerPreset> _presets;

	public IdentifierResolver(List<TimerPreset> presets)
	{
		_presets = presets;
	}

	public (bool Success, TimerPreset Preset, string Error) Resolve(string identifier)
	{
		if (string.IsNullOrWhiteSpace(identifier))
			return (false, null, "Identifier is empty");

		// 1. Try Guid
		if (Guid.TryParse(identifier, out var guid)) {
			var preset = _presets.FirstOrDefault(p => p.Id == guid);
			return preset != null
				? (true, preset, null)
				: (false, null, $"No preset with ID '{identifier}'");
		}

		// 2. DisplayLabel (case-insensitive, skip empty)
		var labelMatches = _presets
			.Where(p => !string.IsNullOrWhiteSpace(p.DisplayLabel) && p.DisplayLabel.Equals(identifier, StringComparison.OrdinalIgnoreCase))
			.ToList();

		if (labelMatches.Count == 1)
			return (true, labelMatches[0], null);
		if (labelMatches.Count > 1)
			return (false, null, $"Ambiguous: {labelMatches.Count} presets match label '{identifier}'");

		// 3. ProcessName base name without extension (e.g. "discord" matches "discord.exe")
		var baseMatches = _presets
			.Where(p => ProcessNameHelper.GetBaseNameWithoutExtension(p.ProcessName).Equals(identifier, StringComparison.OrdinalIgnoreCase))
			.ToList();

		if (baseMatches.Count == 1)
			return (true, baseMatches[0], null);
		if (baseMatches.Count > 1)
			return (false, null, $"Ambiguous: {baseMatches.Count} presets match process '{identifier}'");

		// 4. ProcessName exe name (e.g. "discord.exe" matches "discord.exe")
		var exeMatches = _presets
			.Where(p => ProcessNameHelper.GetExeName(p.ProcessName).Equals(identifier, StringComparison.OrdinalIgnoreCase))
			.ToList();

		if (exeMatches.Count == 1)
			return (true, exeMatches[0], null);
		if (exeMatches.Count > 1)
			return (false, null, $"Ambiguous: {exeMatches.Count} presets match exe '{identifier}'");

		return (false, null, $"No preset found matching '{identifier}'");
	}
}
