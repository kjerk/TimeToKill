using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TimeToKill.App.ViewModels;

public partial class TimerGroupViewModel : ViewModelBase
{
	public string GroupName { get; }
	public ObservableCollection<TimerPresetViewModel> Items { get; }
	public int Count => Items.Count;

	[ObservableProperty]
	private bool _isExpanded = true;

	public TimerGroupViewModel(string groupName, IEnumerable<TimerPresetViewModel> items)
	{
		GroupName = groupName;
		Items = new ObservableCollection<TimerPresetViewModel>(items);
	}

	[RelayCommand]
	private void ToggleExpanded()
	{
		IsExpanded = !IsExpanded;
	}
}
