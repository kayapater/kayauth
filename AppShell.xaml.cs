using AuthApp.Views;

namespace AuthApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("WelcomePage", typeof(WelcomePage));
		Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));
		Routing.RegisterRoute("QrScannerPage", typeof(QrScannerPage));
	}
}
