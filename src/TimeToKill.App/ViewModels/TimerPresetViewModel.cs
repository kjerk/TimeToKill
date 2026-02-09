using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeToKill.App.Services;
using TimeToKill.Models;
using TimeToKill.Tools;

namespace TimeToKill.App.ViewModels;

public partial class TimerPresetViewModel : ViewModelBase
{
	private TimerPreset _preset;
	private readonly TimerManager _timerManager;
	private readonly PresetRepository _presetRepository;
	private readonly Action<TimerPresetViewModel> _onDelete;
	private readonly Action<TimerPresetViewModel> _onEdit;

	[ObservableProperty]
	private bool _isRunning;

	[ObservableProperty]
	private bool _isPaused;

	[ObservableProperty]
	private TimeSpan _remainingTime;

	[ObservableProperty]
	private double _progressPercentage;

	public Guid Id => _preset.Id;
	public string ProcessName => _preset.ProcessName;
	public TimeSpan Duration => _preset.Duration;
	public TimerActionType ActionType => _preset.ActionType;
	public bool AutoRunOnStart => _preset.AutoRunOnStart;

	public string DisplayName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(_preset.DisplayLabel))
				return _preset.DisplayLabel;
			return ProcessNameHelper.GetExeName(_preset.ProcessName);
		}
	}

	public string ActionTypeDisplay => ActionType.GetDescription();

	public string DurationDisplay
	{
		get
		{
			var parts = new List<string>();
			if (Duration.Hours > 0) parts.Add($"{Duration.Hours}h");
			if (Duration.Minutes > 0) parts.Add($"{Duration.Minutes}m");
			if (Duration.Seconds > 0 || parts.Count == 0) parts.Add($"{Duration.Seconds}s");
			return string.Join(" ", parts);
		}
	}

	public string RemainingDisplay
	{
		get
		{
			if (!IsRunning && !IsPaused) return string.Empty;
			var parts = new List<string>();
			if (RemainingTime.Hours > 0) parts.Add($"{RemainingTime.Hours}h");
			if (RemainingTime.Minutes > 0) parts.Add($"{RemainingTime.Minutes}m");
			parts.Add($"{RemainingTime.Seconds}s");
			return string.Join(" ", parts);
		}
	}

	public string StatusDisplay
	{
		get
		{
			if (IsRunning) return $"{RemainingDisplay} remaining";
			if (IsPaused) return $"Paused - {RemainingDisplay}";
			return "Stopped";
		}
	}

	public string StateDisplay
	{
		get
		{
			if (IsRunning) return "Running";
			if (IsPaused) return "Paused";
			return "";
		}
	}

	public bool IsActive => IsRunning || IsPaused;

	public string RemainingDisplayFull
	{
		get
		{
			if (!IsRunning && !IsPaused) return "";
			return $"{RemainingDisplay} remaining";
		}
	}

	public TimerPresetViewModel( TimerPreset preset, TimerManager timerManager, PresetRepository presetRepository, Action<TimerPresetViewModel> onDelete, Action<TimerPresetViewModel> onEdit)
	{
		_preset = preset;
		_timerManager = timerManager;
		_presetRepository = presetRepository;
		_onDelete = onDelete;
		_onEdit = onEdit;

		_remainingTime = preset.Duration;

		// Check if there's already an active timer for this preset
		var activeTimer = timerManager.GetActiveTimerByPresetId(preset.Id);
		if (activeTimer != null)
		{
			UpdateFromActiveTimer(activeTimer);
		}

		// Subscribe to timer events
		_timerManager.TimerTick += OnTimerTick;
		_timerManager.TimerCompleted += OnTimerCompleted;
	}

	public TimerPreset GetPreset() => _preset;

	public void RefreshFromPreset(TimerPreset preset)
	{
		_preset = preset;
		RemainingTime = preset.Duration;
		OnPropertyChanged(nameof(ProcessName));
		OnPropertyChanged(nameof(Duration));
		OnPropertyChanged(nameof(DurationDisplay));
		OnPropertyChanged(nameof(ActionType));
		OnPropertyChanged(nameof(ActionTypeDisplay));
		OnPropertyChanged(nameof(AutoRunOnStart));
		OnPropertyChanged(nameof(DisplayName));
	}

	public void UpdateFromActiveTimer(ActiveTimer timer)
	{
		if (timer == null)
		{
			IsRunning = false;
			IsPaused = false;
			RemainingTime = Duration;
			ProgressPercentage = 0;
		}
		else
		{
			IsRunning = timer.State == TimerState.Running;
			IsPaused = timer.State == TimerState.Paused;
			RemainingTime = timer.Remaining;
			ProgressPercentage = timer.ProgressPercentage(Duration);
		}
		OnPropertyChanged(nameof(RemainingDisplay));
		OnPropertyChanged(nameof(StatusDisplay));
		OnPropertyChanged(nameof(StateDisplay));
		OnPropertyChanged(nameof(RemainingDisplayFull));
		OnPropertyChanged(nameof(IsActive));
	}

	private void OnTimerTick(object sender, TimerTickEventArgs e)
	{
		if (e.Timer.PresetId == Id)
		{
			UpdateFromActiveTimer(e.Timer);
		}
	}

	private void OnTimerCompleted(object sender, TimerCompletedEventArgs e)
	{
		if (e.Timer.PresetId == Id)
		{
			IsRunning = false;
			IsPaused = false;
			RemainingTime = Duration;
			ProgressPercentage = 0;
			OnPropertyChanged(nameof(RemainingDisplay));
			OnPropertyChanged(nameof(StatusDisplay));
			OnPropertyChanged(nameof(StateDisplay));
			OnPropertyChanged(nameof(RemainingDisplayFull));
			OnPropertyChanged(nameof(IsActive));
		}
	}

	[RelayCommand]
	private void Start()
	{
		var timer = _timerManager.Start(_preset);
		UpdateFromActiveTimer(timer);
	}

	[RelayCommand]
	private void Pause()
	{
		_timerManager.Pause(Id);
		var timer = _timerManager.GetActiveTimerByPresetId(Id);
		UpdateFromActiveTimer(timer);
	}

	[RelayCommand]
	private void Resume()
	{
		_timerManager.Resume(Id);
		var timer = _timerManager.GetActiveTimerByPresetId(Id);
		UpdateFromActiveTimer(timer);
	}

	[RelayCommand]
	private void Cancel()
	{
		_timerManager.Cancel(Id);
		UpdateFromActiveTimer(null);
	}

	[RelayCommand]
	private void AddTime()
	{
		_timerManager.AddTime(Id, Duration);
		var timer = _timerManager.GetActiveTimerByPresetId(Id);
		UpdateFromActiveTimer(timer);
	}

	[RelayCommand]
	private void Edit()
	{
		_onEdit?.Invoke(this);
	}

	[RelayCommand]
	private void Delete()
	{
		// Cancel any active timer first
		_timerManager.Cancel(Id);

		// Unsubscribe from events
		_timerManager.TimerTick -= OnTimerTick;
		_timerManager.TimerCompleted -= OnTimerCompleted;

		// Remove from repository
		_presetRepository.RemovePreset(Id);

		// Notify parent to remove from collection
		_onDelete?.Invoke(this);
	}
}
