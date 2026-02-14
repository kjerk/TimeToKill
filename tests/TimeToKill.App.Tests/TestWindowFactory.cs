using TimeToKill.App.Services;
using TimeToKill.App.ViewModels;
using TimeToKill.App.Views;

namespace TimeToKill.App.Tests;

// Creates fully wired MainWindow instances pointing at test fixture data.
// No processes are actually launched or killed - TimerActionService is real but
// screenshot tests never fire timers to completion.
public static class TestWindowFactory
{
	private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
	
	public static (MainWindow Window, MainWindowViewModel ViewModel) Create(string fixtureFile = "presets-example.json")
	{
		var filePath = Path.Combine(TestDataDir, fixtureFile);
		var repo = new PresetRepository(filePath);
		var actionService = new TimerActionService();
		var timerManager = new TimerManager(actionService, repo);
		var viewModel = new MainWindowViewModel(repo, timerManager);
		var window = new MainWindow { DataContext = viewModel };
		return (window, viewModel);
	}
	
	// Creates a window with no presets loaded (empty state).
	public static (MainWindow Window, MainWindowViewModel ViewModel) CreateEmpty()
	{
		// Point at a nonexistent file - PresetRepository returns empty list
		var repo = new PresetRepository(Path.Combine(TestDataDir, "nonexistent.json"));
		var actionService = new TimerActionService();
		var timerManager = new TimerManager(actionService, repo);
		var viewModel = new MainWindowViewModel(repo, timerManager);
		var window = new MainWindow { DataContext = viewModel };
		return (window, viewModel);
	}
}
