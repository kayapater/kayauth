using AuthApp.Services;

namespace AuthApp;

public partial class App : Application
{
    private readonly IAccountStorageService _storageService;
    private static readonly string _logPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AuthApp_debug.log");

    public App(IAccountStorageService storageService)
	{
        Log("App constructor start");
		InitializeComponent();
        Log("InitializeComponent done");
        _storageService = storageService;

        try
        {
            Helpers.ThemeManager.InitializeTheme(this);
            Log("ThemeManager done");
        }
        catch (Exception ex)
        {
            Log($"ThemeManager FAILED: {ex}");
        }

		MainPage = new AppShell();
        Log("AppShell set");
	}

    protected override async void OnStart()
    {
        Log("OnStart begin");
        base.OnStart();
        
        try
        {
            // Check if accounts exist to decide where to stay/go
            var accounts = await _storageService.GetAccountsAsync();
            Log($"Accounts count: {accounts.Count}");
            if (accounts.Count == 0)
            {
                // No accounts? Redirect to welcome page immediately
                await Shell.Current.GoToAsync("//WelcomePage");
                Log("Navigated to WelcomePage");
            }
        }
        catch (Exception ex)
        {
            Log($"OnStart FAILED: {ex}");
        }
        // Else: App stays on MainPage (default)
    }

    protected override Window CreateWindow(IActivationState? activationState)
	{
        Log("CreateWindow");
		var window = base.CreateWindow(activationState);

#if WINDOWS
		window.Width = 420;
		window.Height = 750;
#endif

		return window;
	}

    private static void Log(string msg)
    {
        try
        {
            System.IO.File.AppendAllText(_logPath, $"[{DateTime.Now:HH:mm:ss.fff}] {msg}\n");
        }
        catch { }
    }
}
