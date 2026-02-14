using System.Collections.Generic;
using CommandLine;

namespace TimeToKill.App.Cli;

public class CliOptions
{
	[Option("start-timer", Required = false, HelpText = "Start timer(s) by GUID or unique name. Repeatable.")]
	public IEnumerable<string> StartTimers { get; set; }

	// Future:
	// [Option("stop-timer", Required = false, HelpText = "Stop timer(s) by GUID or unique name. Repeatable.")]
	// public IEnumerable<string> StopTimers { get; set; }

	// [Option("list", Required = false, Default = false, HelpText = "List all configured timer presets.")]
	// public bool List { get; set; }

	public bool HasCommands => StartTimers?.GetEnumerator().MoveNext() == true;
	
	[Option("help", Default = false, HelpText = "Display this help text.")]
	public bool Help { get; set; }
}
