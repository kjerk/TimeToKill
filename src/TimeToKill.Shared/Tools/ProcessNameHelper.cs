namespace TimeToKill.Tools;

public static class ProcessNameHelper
{
	// "C:/Apps/discord.exe" -> "discord.exe", "discord.exe" -> "discord.exe"
	public static string GetExeName(string processName)
	{
		if (string.IsNullOrWhiteSpace(processName)) return processName;
		return Path.GetFileName(processName);
	}

	// "C:/Apps/discord.exe" -> "discord", "discord.exe" -> "discord"
	public static string GetBaseNameWithoutExtension(string processName)
	{
		if (string.IsNullOrWhiteSpace(processName)) return processName;
		return Path.GetFileNameWithoutExtension(processName);
	}

	// Returns true if the processName looks like a full path
	public static bool IsFullPath(string processName)
	{
		if (string.IsNullOrWhiteSpace(processName)) return false;
		return processName.Contains('/') || processName.Contains('\\');
	}
}
