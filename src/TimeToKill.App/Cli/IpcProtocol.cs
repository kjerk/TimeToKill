using System.Collections.Generic;
using System.Linq;

namespace TimeToKill.App.Cli;

public enum IpcCommandType
{
	StartTimer = 1,
	// Future: StopTimer = 2, ListTimers = 3
}

public class IpcCommand
{
	public IpcCommandType CommandType { get; set; }
	public List<string> Arguments { get; set; } = new();

	public static IpcCommand StartTimer(IEnumerable<string> identifiers)
	{
		return new IpcCommand {
			CommandType = IpcCommandType.StartTimer,
			Arguments = identifiers.ToList()
		};
	}
}
