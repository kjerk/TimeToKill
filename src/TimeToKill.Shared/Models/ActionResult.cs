namespace TimeToKill.Models;

// Result of a timer's action firing.
public class ActionResult
{
	public bool Success { get; set; }
	public string ProcessName { get; set; } = string.Empty;
	public int ProcessesAffected { get; set; }
	public string Message { get; set; } = string.Empty;
	public TimerActionType ActionType { get; set; }
	
	public static ActionResult Succeeded(string processName, TimerActionType actionType, int count, string message = null)
	{
		return new ActionResult
		{
			Success = true,
			ProcessName = processName,
			ActionType = actionType,
			ProcessesAffected = count,
			Message = message ?? $"{actionType} completed successfully"
		};
	}

	public static ActionResult Failed(string processName, TimerActionType actionType, string error)
	{
		return new ActionResult
		{
			Success = false,
			ProcessName = processName,
			ActionType = actionType,
			ProcessesAffected = 0,
			Message = error
		};
	}
}
