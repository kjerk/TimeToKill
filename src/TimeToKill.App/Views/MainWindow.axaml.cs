using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TimeToKill.App.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
		Closing += OnClosing;
		Loaded += OnLoaded;
	}
	
	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		var themeService = App.ThemeService;
		if (themeService != null) {
			ThemeSelector.ItemsSource = themeService.AvailableThemes;
			ThemeSelector.SelectedItem = themeService.CurrentTheme;
		}
	}

	private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (ThemeSelector.SelectedItem is string themeName) {
			App.ThemeService?.ApplyTheme(themeName);
		}
	}
	
	private void OnClosing(object sender, WindowClosingEventArgs e)
	{
		// Instead of closing, hide to tray
		e.Cancel = true;
		Hide();
	}
}
