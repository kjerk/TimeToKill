namespace TimeToKill.Models;

/// <summary>
/// Runtime state for a running countdown timer. Not persisted.
/// </summary>
public class ActiveTimer
{
	public Guid PresetId { get; set; }
	public string ProcessName { get; set; }
	public TimeSpan Remaining { get; set; }
	public TimerState State { get; set; }
	public DateTime StartedAt { get; set; }
	public DateTime? CompletedAt { get; set; }
	
	public ActiveTimer()
	{
		ProcessName = string.Empty;
		State = TimerState.Running;
		StartedAt = DateTime.UtcNow;
	}
	
	public ActiveTimer(TimerPreset preset)
	{
		PresetId = preset.Id;
		ProcessName = preset.ProcessName;
		Remaining = preset.Duration;
		State = TimerState.Running;
		StartedAt = DateTime.UtcNow;
	}
	
	public double ProgressPercentage(TimeSpan totalDuration)
	{
		if (totalDuration == TimeSpan.Zero) return 100;
		var elapsed = totalDuration - Remaining;
		return (elapsed.TotalSeconds / totalDuration.TotalSeconds) * 100;
	}
}
