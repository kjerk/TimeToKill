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
			_ => ActionResult.Failed(processName, actionType, $"Unknown action type: {actionType}")
		};
	}
	
	private ActionResult ExecuteKill(string processName, bool force)
	{
		var (success, count, error) = ProcessTools.KillByName(processName, force);
		var actionType = force ? TimerActionType.KillForce : TimerActionType.Kill;
		
		return success
			? ActionResult.Succeeded(processName, actionType, count, $"Terminated {count} process(es)")
			: ActionResult.Failed(processName, actionType, error);
	}
	
	private ActionResult ExecuteSuspend(string processName)
	{
		var (success, count, error) = ProcessTools.SuspendByName(processName);
		
		return success
			? ActionResult.Succeeded(processName, TimerActionType.Suspend, count,
				$"Suspended {count} process(es)")
			: ActionResult.Failed(processName, TimerActionType.Suspend, error);
	}
	
	private ActionResult ExecuteDemotePriority(string processName)
	{
		var (success, count, error) = ProcessTools.DemotePriorityByName(processName);
		
		return success
			? ActionResult.Succeeded(processName, TimerActionType.DemotePriority, count,
				$"Demoted priority of {count} process(es)")
			: ActionResult.Failed(processName, TimerActionType.DemotePriority, error);
	}
}
