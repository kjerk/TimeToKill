using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.Threading;
using TimeToKill.Models;

namespace TimeToKill.App.Services;

public class TimerTickEventArgs : EventArgs
{
	public ActiveTimer Timer { get; }
	
	public TimerTickEventArgs(ActiveTimer timer)
	{
		Timer = timer;
	}
}

public class TimerCompletedEventArgs : EventArgs
{
	public ActiveTimer Timer { get; }
	public ActionResult Result { get; }
	
	public TimerCompletedEventArgs(ActiveTimer timer, ActionResult result)
	{
		Timer = timer;
		Result = result;
	}
}

public class TimerManager
{
	private readonly TimerActionService _actionService;
	private readonly PresetRepository _presetRepository;
	private readonly List<ActiveTimer> _activeTimers;
	private readonly DispatcherTimer _ticker;
	private readonly Lock _lock = new Lock();
	
	public event EventHandler<TimerTickEventArgs> TimerTick;
	public event EventHandler<TimerCompletedEventArgs> TimerCompleted;
	
	public bool HasActiveTimers
	{
		get {
			lock (_lock) {
				return _activeTimers.Count > 0;
			}
		}
	}
	
	public bool HasRunningTimers
	{
		get {
			lock (_lock) {
				return _activeTimers.Any(t => t.State == TimerState.Running);
			}
		}
	}
	
	public TimerManager(TimerActionService actionService, PresetRepository presetRepository)
	{
		_actionService = actionService;
		_presetRepository = presetRepository;
		_activeTimers = new List<ActiveTimer>();
		
		_ticker = new DispatcherTimer {
			Interval = TimeSpan.FromSeconds(1)
		};
		_ticker.Tick += OnTick;
	}
	
	public ActiveTimer Start(TimerPreset preset)
	{
		lock (_lock) {
			// Check if there's already a timer for this process
			var existing = _activeTimers.FirstOrDefault(t =>
				t.ProcessName.Equals(preset.ProcessName, StringComparison.OrdinalIgnoreCase) &&
				t.State != TimerState.Completed);
			
			if (existing != null) {
				// Cancel the existing timer before starting a new one
				existing.State = TimerState.Completed;
				existing.CompletedAt = DateTime.UtcNow;
			}
			
			var timer = new ActiveTimer(preset);
			_activeTimers.Add(timer);
			
			// Start the ticker if not already running
			if (!_ticker.IsEnabled) {
				_ticker.Start();
			}
			
			return timer;
		}
	}
	
	public void Pause(Guid presetId)
	{
		lock (_lock) {
			var timer = _activeTimers.FirstOrDefault(t => t.PresetId == presetId);
			if (timer != null && timer.State == TimerState.Running) {
				timer.State = TimerState.Paused;
			}
		}
	}
	
	public void Resume(Guid presetId)
	{
		lock (_lock) {
			var timer = _activeTimers.FirstOrDefault(t => t.PresetId == presetId);
			if (timer != null && timer.State == TimerState.Paused) {
				timer.State = TimerState.Running;
				
				if (!_ticker.IsEnabled) {
					_ticker.Start();
				}
			}
		}
	}
	
	public void Cancel(Guid presetId)
	{
		lock (_lock) {
			var timer = _activeTimers.FirstOrDefault(t => t.PresetId == presetId);
			if (timer == null) return;
			_activeTimers.Remove(timer);
			StopTickerIfNoActive();
		}
	}
	
	public IReadOnlyList<ActiveTimer> GetActiveTimers()
	{
		lock (_lock) {
			return _activeTimers.ToList().AsReadOnly();
		}
	}
	
	public ActiveTimer GetActiveTimerForProcess(string processName)
	{
		lock (_lock) {
			return _activeTimers.FirstOrDefault(t =>
				t.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase) &&
				t.State != TimerState.Completed);
		}
	}
	
	public ActiveTimer GetActiveTimerByPresetId(Guid presetId)
	{
		lock (_lock) {
			return _activeTimers.FirstOrDefault(t => t.PresetId == presetId && t.State != TimerState.Completed);
		}
	}
	
	private void OnTick(object sender, EventArgs e)
	{
		var completedTimers = new List<ActiveTimer>();
		var tickedTimers = new List<ActiveTimer>();
		
		lock (_lock) {
			foreach (var timer in _activeTimers.Where(t => t.State == TimerState.Running).ToList()) {
				timer.Remaining = timer.Remaining.Subtract(TimeSpan.FromSeconds(1));
				
				if (timer.Remaining <= TimeSpan.Zero) {
					timer.Remaining = TimeSpan.Zero;
					timer.State = TimerState.Completed;
					timer.CompletedAt = DateTime.UtcNow;
					completedTimers.Add(timer);
				} else {
					tickedTimers.Add(timer);
				}
			}
		}
		
		// Tick events run outside of the lock
		foreach (var timer in tickedTimers) {
			TimerTick?.Invoke(this, new TimerTickEventArgs(timer));
		}
		
		// Then also process any completed timers
		foreach (var timer in completedTimers) {
			var presets = _presetRepository.LoadPresets();
			var preset = presets.FirstOrDefault(p => p.Id == timer.PresetId);
			var actionType = preset?.ActionType ?? TimerActionType.Kill;
			var actionArgs = preset?.ActionArgs;
			
			var result = _actionService.Execute(timer.ProcessName, actionType, actionArgs);
			TimerCompleted?.Invoke(this, new TimerCompletedEventArgs(timer, result));
		}
		
		lock (_lock) {
			// Remove completed timers from active list
			_activeTimers.RemoveAll(t => t.State == TimerState.Completed);
			StopTickerIfNoActive();
		}
	}
	
	private void StopTickerIfNoActive()
	{
		if (!_activeTimers.Any(t => t.State == TimerState.Running)) {
			_ticker.Stop();
		}
	}
}
