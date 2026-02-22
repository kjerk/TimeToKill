using System.Runtime.InteropServices;

namespace TimeToKill.Tools;

public static class NativeMethods
{
	private const int AttachParentProcess = -1;

	[DllImport("kernel32.dll")]
	private static extern bool AttachConsole(int dwProcessId);

	public static bool AttachToParentConsole() => AttachConsole(AttachParentProcess);
}
