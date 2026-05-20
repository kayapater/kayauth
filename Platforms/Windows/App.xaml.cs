using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AuthApp.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		this.InitializeComponent();
		this.UnhandledException += OnUnhandledException;
	}

	private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{
		try
		{
			var logPath = System.IO.Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
				"AuthApp_crash.log");
			var message = $"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] CRASH: {e.Exception}\n{e.Exception?.StackTrace}\n---\n";
			System.IO.File.AppendAllText(logPath, message);
		}
		catch { }
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

