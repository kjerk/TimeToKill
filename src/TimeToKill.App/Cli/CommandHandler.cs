using System;
using System.Linq;
using Avalonia.Threading;
using TimeToKill.App.Services;
using TimeToKill.App.ViewModels;

namespace TimeToKill.App.Cli;

public class CommandHandler
{
	private readonly PresetRepository _presetRepository;
	private readonly MainWindowViewModel _viewModel;

	public CommandHandler(PresetRepository presetRepository, MainWindowViewModel viewModel)
	{
		_presetRepository = presetRepository;
		_viewModel = viewModel;
	}

	public void Process(IpcCommand command)
	{
		switch (command.CommandType) {
			case IpcCommandType.StartTimer:
				HandleStartTimer(command);
				break;
		}
	}

	private void HandleStartTimer(IpcCommand command)
	{
		var presets = _presetRepository.LoadPresets();
		var resolver = new IdentifierResolver(presets);

		foreach (var identifier in command.Arguments) {
			var (success, preset, error) = resolver.Resolve(identifier);

			if (!success) {
				Notify(error);
				continue;
			}

			Dispatcher.UIThread.Post(() => {
				var vm = _viewModel.Presets.FirstOrDefault(p => p.Id == preset.Id);
				if (vm != null) {
					vm.StartCommand.Execute(null);
					var name = preset.DisplayLabel;
					if (string.IsNullOrWhiteSpace(name))
						name = preset.ProcessName;
					Notify($"Started timer: {name}");
				} else {
					Notify($"Preset found but not loaded: {identifier}");
				}
			});
		}
	}

	private void Notify(string message)
	{
		Dispatcher.UIThread.Post(() => {
			_viewModel.LastNotification = message;
			_viewModel.HasNotification = true;
		});
	}
}
