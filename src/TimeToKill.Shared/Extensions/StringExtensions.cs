namespace TimeToKill.Extensions;

public static class StringExtensions
{
	public static bool HasValue(this string str)
	{
		return !string.IsNullOrWhiteSpace(str);
	}
	
	public static string RejectBackslashes(this string str, bool fallbackToEmpty = true)
	{
		if (str == null) return fallbackToEmpty ? string.Empty : null;
		return str.Replace('\\', '/');
	}
}
