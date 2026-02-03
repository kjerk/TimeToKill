namespace TimeToKill.Extensions;

public static class StringExtensions
{
	public static bool HasValue(this string str)
	{
		return !string.IsNullOrWhiteSpace(str);
	}
}
