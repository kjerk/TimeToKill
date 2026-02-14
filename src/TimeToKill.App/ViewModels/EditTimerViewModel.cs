using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeToKill.App.Services;
using TimeToKill.Extensions;
using TimeToKill.Models;

namespace TimeToKill.App.ViewModels;

public class ActionTypeOption
{
	public TimerActionType Value { get; }
	public string DisplayName { get; }

	public ActionTypeOption(TimerActionType value)
	{
		Value = value;
		DisplayName = value.GetDescription();
	}

	public override string ToString() => DisplayName;
}

public partial class EditTimerViewModel : ViewModelBase
{
	private readonly PresetRepository _presetRepository;
	private readonly Action<TimerPreset, bool> _onSave;
	private readonly Action _onCancel;
	private readonly Guid? _existingId;

	public bool IsNewTimer => _existingId == null;
	public string Title => IsNewTimer ? "New Timer" : "Edit Timer";

	public static IReadOnlyList<ActionTypeOption> ActionTypeOptions { get; } =
		Enum.GetValues<TimerActionType>().Select(t => new ActionTypeOption(t)).ToList();

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
	private string _processName = string.Empty;

	[ObservableProperty]
	private string _displayLabel = string.Empty;

	private int _hours;
	private int _minutes;
	private int _seconds;

	public string HoursText
	{
		get => _hours > 0 ? _hours.ToString() : "";
		set
		{
			if (int.TryParse(value, out var h) && h >= 0 && h <= 23)
				_hours = h;
			else if (string.IsNullOrEmpty(value))
				_hours = 0;
			OnPropertyChanged();
			SaveCommand.NotifyCanExecuteChanged();
		}
	}

	public string MinutesText
	{
		get => _minutes > 0 || _hours > 0 ? _minutes.ToString() : "";
		set
		{
			if (int.TryParse(value, out var m) && m >= 0 && m <= 59)
				_minutes = m;
			else if (string.IsNullOrEmpty(value))
				_minutes = 0;
			OnPropertyChanged();
			SaveCommand.NotifyCanExecuteChanged();
		}
	}

	public string SecondsText
	{
		get => _seconds > 0 || _minutes > 0 || _hours > 0 ? _seconds.ToString() : "";
		set
		{
			if (int.TryParse(value, out var s) && s >= 0 && s <= 59)
				_seconds = s;
			else if (string.IsNullOrEmpty(value))
				_seconds = 0;
			OnPropertyChanged();
			SaveCommand.NotifyCanExecuteChanged();
		}
	}

	[ObservableProperty]
	private ActionTypeOption _selectedActionType;
	
	[ObservableProperty]
	private bool _autoRunOnStart;
	
	[ObservableProperty]
	private string _validationError = string.Empty;
	
	public bool HasValidationError => !string.IsNullOrEmpty(ValidationError);
	
	public EditTimerViewModel(PresetRepository presetRepository, TimerPreset existingPreset, Action<TimerPreset, bool> onSave, Action onCancel)
	{
		_presetRepository = presetRepository;
		_onSave = onSave;
		_onCancel = onCancel;

		if (existingPreset != null) {
			_existingId = existingPreset.Id;
			_processName = existingPreset.ProcessName;
			_displayLabel = existingPreset.DisplayLabel ?? string.Empty;
			_hours = existingPreset.Duration.Hours;
			_minutes = existingPreset.Duration.Minutes;
			_seconds = existingPreset.Duration.Seconds;
			_selectedActionType = ActionTypeOptions.First(o => o.Value == existingPreset.ActionType);
			_autoRunOnStart = existingPreset.AutoRunOnStart;
		} else {
			_minutes = 5;
			_selectedActionType = ActionTypeOptions[0];
		}
	}
	
	private bool CanSave()
	{
		return !string.IsNullOrWhiteSpace(ProcessName) && GetTotalDuration() > TimeSpan.Zero;
	}
	
	private TimeSpan GetTotalDuration()
	{
		return new TimeSpan(_hours, _minutes, _seconds);
	}
	
	private bool Validate()
	{
		ValidationError = string.Empty;
		
		if (string.IsNullOrWhiteSpace(ProcessName)) {
			ValidationError = "Process name is required";
			OnPropertyChanged(nameof(HasValidationError));
			return false;
		}
		
		if (GetTotalDuration() <= TimeSpan.Zero) {
			ValidationError = "Duration must be greater than zero";
			OnPropertyChanged(nameof(HasValidationError));
			return false;
		}
		
		OnPropertyChanged(nameof(HasValidationError));
		return true;
	}
	
	[RelayCommand(CanExecute = nameof(CanSave))]
	private void Save()
	{
		if (!Validate()) return;

		TimerPreset preset;
		bool isNew;
		var actionType = SelectedActionType?.Value ?? TimerActionType.Kill;

		if (_existingId.HasValue) {
			preset = new TimerPreset {
				Id = _existingId.Value,
				ProcessName = ProcessName.Trim().RejectBackslashes(),
				DisplayLabel = DisplayLabel?.Trim() ?? string.Empty,
				Duration = GetTotalDuration(),
				ActionType = actionType,
				AutoRunOnStart = AutoRunOnStart,
				CreatedAt = DateTime.UtcNow
			};
			_presetRepository.UpdatePreset(preset);
			isNew = false;
		} else {
			preset = new TimerPreset(ProcessName.Trim().RejectBackslashes(), GetTotalDuration(), actionType) {
				DisplayLabel = DisplayLabel?.Trim() ?? string.Empty,
				AutoRunOnStart = AutoRunOnStart
			};
			_presetRepository.AddPreset(preset);
			isNew = true;
		}

		_onSave?.Invoke(preset, isNew);
	}
	
	[RelayCommand]
	private void Cancel()
	{
		_onCancel?.Invoke();
	}

	[RelayCommand]
	private void SetDuration(string secondsStr)
	{
		if (!int.TryParse(secondsStr, out var totalSeconds)) return;
		var ts = TimeSpan.FromSeconds(totalSeconds);
		_hours = (int)ts.TotalHours;
		_minutes = ts.Minutes;
		_seconds = ts.Seconds;
		OnPropertyChanged(nameof(HoursText));
		OnPropertyChanged(nameof(MinutesText));
		OnPropertyChanged(nameof(SecondsText));
		SaveCommand.NotifyCanExecuteChanged();
	}
}
