using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeToKill.App.Services;
using TimeToKill.Models;

namespace TimeToKill.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	private readonly PresetRepository _presetRepository;
	private readonly TimerManager _timerManager;
	
	[ObservableProperty]
	private ObservableCollection<TimerPresetViewModel> _presets = new();
	
	[ObservableProperty]
	private bool _isEditingTimer;
	
	[ObservableProperty]
	private EditTimerViewModel _editTimerViewModel;
	
	[ObservableProperty]
	private string _lastNotification = string.Empty;
	
	[ObservableProperty]
	private bool _hasNotification;
	
	public bool HasPresets => Presets.Count > 0;
	public bool HasActiveTimers => _timerManager?.HasActiveTimers ?? false;
	public bool HasRunningTimers => _timerManager?.HasRunningTimers ?? false;
	
	public string TrayTooltipText
	{
		get {
			var activeTimers = _timerManager?.GetActiveTimers();
			if (activeTimers == null || activeTimers.Count == 0) {
				return "TimeToKill - No active timers";
			}
			
			var lines = new List<string> { "TimeToKill" };
			foreach (var timer in activeTimers.Where(t => t.State != TimerState.Completed)) {
				var status = timer.State == TimerState.Paused ? " (paused)" : "";
				var remaining = FormatTimeSpan(timer.Remaining);
				lines.Add($"{timer.ProcessName} - {remaining}{status}");
			}
			
			return string.Join("\n", lines);
		}
	}
	
	public MainWindowViewModel()
	{
		// Design-time constructor
		_presetRepository = new PresetRepository();
		var actionService = new TimerActionService();
		_timerManager = new TimerManager(actionService, _presetRepository);
	}
	
	public MainWindowViewModel(PresetRepository presetRepository, TimerManager timerManager)
	{
		_presetRepository = presetRepository;
		_timerManager = timerManager;
		
		LoadPresets();
		
		_timerManager.TimerTick += OnTimerTick;
		_timerManager.TimerCompleted += OnTimerCompleted;
	}
	
	private void LoadPresets()
	{
		var presets = _presetRepository.LoadPresets();
		Presets.Clear();
		
		foreach (var preset in presets.OrderBy(p => p.CreatedAt)) {
			var vm = new TimerPresetViewModel(preset, _timerManager, _presetRepository, OnPresetDeleted, OnPresetEdit);
			Presets.Add(vm);
		}
		
		OnPropertyChanged(nameof(HasPresets));
		
		// Auto-start timers that have AutoRunOnStart enabled
		foreach (var presetVm in Presets.Where(p => p.AutoRunOnStart)) {
			presetVm.StartCommand.Execute(null);
		}
	}
	
	private void OnPresetDeleted(TimerPresetViewModel presetVm)
	{
		Presets.Remove(presetVm);
		OnPropertyChanged(nameof(HasPresets));
	}
	
	private void OnPresetEdit(TimerPresetViewModel presetVm)
	{
		EditTimerViewModel = new EditTimerViewModel(_presetRepository, presetVm.GetPreset(), OnTimerSaved, OnEditTimerCancelled);
		IsEditingTimer = true;
	}
	
	private void OnTimerTick(object sender, TimerTickEventArgs e)
	{
		OnPropertyChanged(nameof(TrayTooltipText));
		OnPropertyChanged(nameof(HasActiveTimers));
		OnPropertyChanged(nameof(HasRunningTimers));
	}
	
	private void OnTimerCompleted(object sender, TimerCompletedEventArgs e)
	{
		OnPropertyChanged(nameof(TrayTooltipText));
		OnPropertyChanged(nameof(HasActiveTimers));
		OnPropertyChanged(nameof(HasRunningTimers));
		
		// Show notification about the action result
		var result = e.Result;
		if (result.Success) {
			var count = result.ProcessesAffected;
			var plural = count == 1 ? "" : "es";
			LastNotification = $"{result.ActionType}: {result.ProcessName} ({count} process{plural})";
		} else {
			LastNotification = $"Failed: {result.ProcessName} - {result.Message}";
		}
		
		HasNotification = true;
	}
	
	[RelayCommand]
	private void OpenAddTimer()
	{
		EditTimerViewModel = new EditTimerViewModel(_presetRepository, null, OnTimerSaved, OnEditTimerCancelled);
		IsEditingTimer = true;
	}
	
	private void OnTimerSaved(TimerPreset preset, bool isNew)
	{
		if (isNew) {
			var vm = new TimerPresetViewModel(preset, _timerManager, _presetRepository, OnPresetDeleted, OnPresetEdit);
			Presets.Add(vm);
		} else {
			// Find and refresh the existing preset VM
			var existingVm = Presets.FirstOrDefault(p => p.Id == preset.Id);
			if (existingVm != null) {
				existingVm.RefreshFromPreset(preset);
			}
		}
		
		OnPropertyChanged(nameof(HasPresets));
		IsEditingTimer = false;
		EditTimerViewModel = null;
	}
	
	private void OnEditTimerCancelled()
	{
		IsEditingTimer = false;
		EditTimerViewModel = null;
	}
	
	[RelayCommand]
	private void DismissNotification()
	{
		HasNotification = false;
		LastNotification = string.Empty;
	}
	
	private static string FormatTimeSpan(TimeSpan ts)
	{
		var parts = new List<string>();
		if (ts.Hours > 0) parts.Add($"{ts.Hours}h");
		if (ts.Minutes > 0) parts.Add($"{ts.Minutes}m");
		parts.Add($"{ts.Seconds}s");
		return string.Join(" ", parts);
	}
}
