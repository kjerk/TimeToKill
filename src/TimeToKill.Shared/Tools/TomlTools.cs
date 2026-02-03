using Tomlyn;
using Tomlyn.Model;

namespace TimeToKill.Tools;

public static class TomlTools
{
	private static readonly TomlModelOptions _defaultOptions = new TomlModelOptions() {
		ConvertPropertyName = s => s,
		ConvertFieldName =  s => s
	};
	
	// Just a passthrough for ToModel but with default options.
	public static T Parse<T>(string tomlContent) where T : class, new()
	{
		return Toml.ToModel<T>(tomlContent, null, _defaultOptions);
	}
	
	// Passthrough for ToModel (generic object) but with default options.
	public static TomlTable Parse(string tomlContent)
	{
		return Toml.ToModel(tomlContent, null, _defaultOptions);
	}
	
	/// <summary>
	/// Parse TOML and extract all sections matching a prefix (e.g., "themes.").
	/// Returns dictionary of section name (without prefix) → key/value pairs.
	/// </summary>
	public static Dictionary<string, Dictionary<string, string>> ExtractSections(string tomlContent, string sectionPrefix)
	{
		var result = new Dictionary<string, Dictionary<string, string>>();
		var model = Parse(tomlContent);
		
		// Look for the root table matching the prefix (e.g., "themes")
		var prefixParts = sectionPrefix.TrimEnd('.').Split('.');
		TomlTable currentTable = model;
		
		foreach (var part in prefixParts) {
			if (currentTable.TryGetValue(part, out var value) && value is TomlTable nested) {
				currentTable = nested;
			} else {
				return result; // Prefix not found
			}
		}
		
		// Now iterate all sub-tables under this prefix
		foreach (var kvp in currentTable) {
			if (kvp.Value is TomlTable sectionTable) {
				var sectionValues = new Dictionary<string, string>();
				foreach (var entry in sectionTable) {
					sectionValues[entry.Key] = entry.Value?.ToString() ?? string.Empty;
				}
				
				result[kvp.Key] = sectionValues;
			}
		}
		
		return result;
	}
	
	/// <summary>
	/// Merge a child dictionary over a base dictionary, returning a new dictionary.
	/// Child values override base values; base values are used as fallback.
	/// </summary>
	public static Dictionary<string, string> MergeOver(Dictionary<string, string> baseDict, Dictionary<string, string> childDict)
	{
		var result = new Dictionary<string, string>(baseDict);
		foreach (var kvp in childDict) {
			result[kvp.Key] = kvp.Value;
		}
		
		return result;
	}
}
