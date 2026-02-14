using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

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
	
	[Option("help", Default = false, HelpText = "Display this help text.")]
	public bool Help { get; set; }
	
	public bool HasCommands => StartTimers?.GetEnumerator().MoveNext() == true;
	
	private static Parser CreateParser() => new Parser(s => {
		s.AutoHelp = false;
		s.AutoVersion = false;
	});
	
	public static CliOptions Parse(string[] args)
	{
		var parseResult = CreateParser().ParseArguments<CliOptions>(args);
		if (parseResult.Tag == ParserResultType.NotParsed)
			return null;
		return parseResult.Value;
	}
	
	public static void PrintHelp()
	{
		var result = CreateParser().ParseArguments<CliOptions>(Array.Empty<string>());
		var helpText = new HelpText("TimeToKill");
		helpText.AddOptions(result);
		Console.WriteLine(helpText);
	}
}
