namespace TimeToKill.Models;

// A persisted timer configuration that survives app restarts.
public class TimerPreset
{
	public Guid Id { get; set; }
	public string ProcessName { get; set; }
	public TimeSpan Duration { get; set; }
	public TimerActionType ActionType { get; set; }
	public string ActionArgs { get; set; }
	public bool AutoRunOnStart { get; set; }
	public DateTime CreatedAt { get; set; }
	
	public TimerPreset()
	{
		Id = Guid.NewGuid();
		ProcessName = string.Empty;
		Duration = TimeSpan.Zero;
		ActionType = TimerActionType.Kill;
		ActionArgs = string.Empty;
		AutoRunOnStart = false;
		CreatedAt = DateTime.UtcNow;
	}
	
	public TimerPreset(string processName, TimeSpan duration, TimerActionType actionType = TimerActionType.Kill)
	{
		Id = Guid.NewGuid();
		ProcessName = processName;
		Duration = duration;
		ActionType = actionType;
		ActionArgs = string.Empty;
		AutoRunOnStart = false;
		CreatedAt = DateTime.UtcNow;
	}
}
