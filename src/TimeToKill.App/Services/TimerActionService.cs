using TimeToKill.Models;
using TimeToKill.Tools;

namespace TimeToKill.App.Services;

public class TimerActionService
{
	public ActionResult Execute(string processName, TimerActionType actionType, string actionArgs = null)
	{
		return actionType switch {
			TimerActionType.Kill => ExecuteKill(processName, force: false),
			TimerActionType.KillForce => ExecuteKill(processName, force: true),
			TimerActionType.Suspend => ExecuteSuspend(processName),
			TimerActionType.DemotePriority => ExecuteDemotePriority(processName),
			TimerActionType.LaunchAndKill => ExecuteKill(processName, force: true),
			_ => ActionResult.Failed(processName, actionType, $"Unknown action type: {actionType}")
		};
	}
	
	private ActionResult ExecuteKill(string processName, bool force)
	{
		var (success, count, error) = ProcessTools.KillByName(processName, force);
		var actionType = force ? TimerActionType.KillForce : TimerActionType.Kill;
		
		if (success)
			return ActionResult.Succeeded(processName, actionType, count, $"Terminated {count} process(es)");
		else
			return ActionResult.Failed(processName, actionType, error);
	}
	
	private ActionResult ExecuteSuspend(string processName)
	{
		var (success, count, error) = ProcessTools.SuspendByName(processName);
		
		if (success)
			return ActionResult.Succeeded(processName, TimerActionType.Suspend, count, $"Suspended {count} process(es)");
		else
			return ActionResult.Failed(processName, TimerActionType.Suspend, error);
	}
	
	private ActionResult ExecuteDemotePriority(string processName)
	{
		var (success, count, error) = ProcessTools.DemotePriorityByName(processName);
		
		if (success)
			return ActionResult.Succeeded(processName, TimerActionType.DemotePriority, count, $"Demoted priority of {count} process(es)");
		else
			return ActionResult.Failed(processName, TimerActionType.DemotePriority, error);
	}

	// Launch a process, skipping if already running. Used by TimerManager on start for LaunchAndKill presets.
	public ActionResult Launch(string processPath)
	{
		var exeName = ProcessNameHelper.GetExeName(processPath);
		if (ProcessTools.IsProcessRunning(exeName)) {
			return ActionResult.Succeeded(exeName, TimerActionType.LaunchAndKill, 0, "Process already running, skipping launch");
		}

		var (success, error) = ProcessTools.LaunchProcess(processPath);
		
		if (success)
			return ActionResult.Succeeded(exeName, TimerActionType.LaunchAndKill, 1, "Launched process");
		else
			return ActionResult.Failed(exeName, TimerActionType.LaunchAndKill, error);
	}
}
