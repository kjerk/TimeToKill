namespace TimeToKill.Models;

// A persisted timer configuration that survives app restarts.
public class TimerPreset
{
	public Guid Id { get; set; }
	public string ProcessName { get; set; }
	public string DisplayLabel { get; set; }
	public TimeSpan Duration { get; set; }
	public TimerActionType ActionType { get; set; }
	public string ActionArgs { get; set; }
	public bool AutoRunOnStart { get; set; }
	public int SortOrder { get; set; }
	public DateTime CreatedAt { get; set; }
	
	public TimerPreset()
	{
		Id = Guid.NewGuid();
		ProcessName = string.Empty;
		DisplayLabel = string.Empty;
		Duration = TimeSpan.Zero;
		ActionType = TimerActionType.Kill;
		ActionArgs = string.Empty;
		AutoRunOnStart = false;
		SortOrder = 0;
		CreatedAt = DateTime.UtcNow;
	}

	public TimerPreset(string processName, TimeSpan duration, TimerActionType actionType = TimerActionType.Kill)
	{
		Id = Guid.NewGuid();
		ProcessName = processName;
		DisplayLabel = string.Empty;
		Duration = duration;
		ActionType = actionType;
		ActionArgs = string.Empty;
		AutoRunOnStart = false;
		SortOrder = 0;
		CreatedAt = DateTime.UtcNow;
	}
}
