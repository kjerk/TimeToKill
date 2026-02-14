using Avalonia.Headless.NUnit;
using TimeToKill.App.ViewModels;

namespace TimeToKill.App.Tests;

[TestFixture]
public class PopulatedStateTests
{
	[AvaloniaTest, Order(1)]
	public void EmptyState_ShowsNoTimersMessage()
	{
		var (window, viewModel) = TestWindowFactory.CreateEmpty();
		window.Show();
		
		Assert.That(viewModel.Presets.Count, Is.EqualTo(0));
		Assert.That(viewModel.HasPresets, Is.False);
		
		var path = ScreenshotHelper.Capture(window, "01_empty-state");
		TestContext.Out.WriteLine($"Screenshot saved: {path}");
		
		window.Close();
	}
	
	[AvaloniaTest, Order(2)]
	public void PopulatedState_RendersWithPresets()
	{
		var (window, viewModel) = TestWindowFactory.Create();
		window.Show();
		
		// Verify presets loaded from fixture
		Assert.That(viewModel.Presets.Count, Is.EqualTo(4));
		Assert.That(viewModel.HasPresets, Is.True);
		
		// Verify grouping - notepad3 and discord each have 2 entries, so 2 groups
		Assert.That(viewModel.GroupedPresets.Count, Is.EqualTo(2));
		Assert.That(viewModel.GroupedPresets[0], Is.InstanceOf<TimerGroupViewModel>());
		Assert.That(viewModel.GroupedPresets[1], Is.InstanceOf<TimerGroupViewModel>());
		
		var path = ScreenshotHelper.Capture(window, "02_populated-state");
		TestContext.Out.WriteLine($"Screenshot saved: {path}");
		
		window.Close();
	}
	
	[AvaloniaTest, Order(3)]
	public void EditForm_RendersWhenOpened()
	{
		var (window, viewModel) = TestWindowFactory.Create();
		window.Show();
		
		// Open the add timer form
		viewModel.OpenAddTimerCommand.Execute(null);
		Assert.That(viewModel.IsEditingTimer, Is.True);
		Assert.That(viewModel.EditTimerViewModel, Is.Not.Null);
		
		var path = ScreenshotHelper.Capture(window, "03_edit-form-open");
		TestContext.Out.WriteLine($"Screenshot saved: {path}");
		
		window.Close();
	}
}
