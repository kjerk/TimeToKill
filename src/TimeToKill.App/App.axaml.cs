using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using TimeToKill.App.Helpers;
using TimeToKill.App.Services;
using TimeToKill.App.Themes;
using TimeToKill.App.ViewModels;
using TimeToKill.App.Views;

namespace TimeToKill.App;

public partial class App : Application
{
	public static MainWindowViewModel MainViewModel { get; private set; }
	public static MainWindow MainWindowInstance { get; private set; }
	public static ThemeService ThemeService { get; private set; }
	
	private PresetRepository _presetRepository;
	private TimerActionService _actionService;
	private TimerManager _timerManager;
	private TrayIcon _trayIcon;
	private Bitmap _currentTrayBitmap;
	private DispatcherTimer _flashTimer;
	
	private const string DefaultTheme = "default-dark";
	
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
		
		ThemeService = new ThemeService();
		// Todo: Load saved theme from settings
		ThemeService.ApplyTheme(DefaultTheme);
	}
	
	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			// Avoid duplicate validations from both Avalonia and the CommunityToolkit.
			DisableAvaloniaDataAnnotationValidation();
			
			// = Services
			_presetRepository = new PresetRepository();
			_actionService = new TimerActionService();
			_timerManager = new TimerManager(_actionService, _presetRepository);
			
			MainViewModel = new MainWindowViewModel(_presetRepository, _timerManager);
			MainWindowInstance = new MainWindow { DataContext = MainViewModel };
			
			SetupTrayIcon();
			
			// Timer events for tray updates
			_timerManager.TimerTick += OnTimerTick;
			_timerManager.TimerCompleted += OnTimerCompleted;
			
			desktop.MainWindow = MainWindowInstance;
			desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
		}
		
		base.OnFrameworkInitializationCompleted();
	}
	
	private void SetupTrayIcon()
	{
		_currentTrayBitmap = TrayIconGenerator.GenerateIcon(TrayStatus.Idle);

		_trayIcon = new TrayIcon {
			Icon = new WindowIcon(_currentTrayBitmap),
			ToolTipText = MainViewModel.TrayTooltipText,
			IsVisible = true,
			Menu = CreateTrayMenu()
		};

		_trayIcon.Clicked += OnTrayIconClicked;

		// Make a new timer specifically for flashing the icon after a timer fires, reusable.
		_flashTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
		_flashTimer.Tick += OnFlashTimerTick;

		var icons = new TrayIcons { _trayIcon };
		TrayIcon.SetIcons(this, icons);
	}

	private void OnFlashTimerTick(object sender, EventArgs e)
	{
		_flashTimer.Stop();
		var status = _timerManager.HasRunningTimers ? TrayStatus.Active : TrayStatus.Idle;
		UpdateTrayIcon(status);
		UpdateTrayTooltip();
	}
	
	private NativeMenu CreateTrayMenu()
	{
		var menu = new NativeMenu();
		
		var showItem = new NativeMenuItem("Show TimeToKill");
		showItem.Click += (s, e) => ShowMainWindow();
		menu.Add(showItem);
		
		menu.Add(new NativeMenuItemSeparator());
		
		var exitItem = new NativeMenuItem("Exit");
		exitItem.Click += (s, e) => ExitApplication();
		menu.Add(exitItem);
		
		return menu;
	}
	
	private void OnTrayIconClicked(object sender, EventArgs e)
	{
		ShowMainWindow();
	}
	
	private void OnTimerTick(object sender, TimerTickEventArgs e)
	{
		Dispatcher.UIThread.Post(() => {
			UpdateTrayIcon(TrayStatus.Active);
			UpdateTrayTooltip();
		});
	}
	
	private void OnTimerCompleted(object sender, TimerCompletedEventArgs e)
	{
		Dispatcher.UIThread.Post(() => {
			UpdateTrayIcon(TrayStatus.Fired);
			UpdateTrayTooltip();

			_flashTimer.Stop();
			_flashTimer.Start();
		});
	}
	
	private void UpdateTrayIcon(TrayStatus status)
	{
		var oldBitmap = _currentTrayBitmap;
		_currentTrayBitmap = TrayIconGenerator.GenerateIcon(status);
		_trayIcon.Icon = new WindowIcon(_currentTrayBitmap);
		oldBitmap?.Dispose();
	}
	
	private void UpdateTrayTooltip()
	{
		_trayIcon.ToolTipText = MainViewModel.TrayTooltipText;
	}
	
	public static void ShowMainWindow()
	{
		if (MainWindowInstance == null) {
			return;
		}
		
		MainWindowInstance.Show();
		MainWindowInstance.WindowState = WindowState.Normal;
		MainWindowInstance.Activate();
	}
	
	public static void ExitApplication()
	{
		if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			desktop.Shutdown();
		}
	}
	
	private void DisableAvaloniaDataAnnotationValidation()
	{
		var dataValidationPluginsToRemove =
			BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
		
		foreach (var plugin in dataValidationPluginsToRemove) {
			BindingPlugins.DataValidators.Remove(plugin);
		}
	}
}
