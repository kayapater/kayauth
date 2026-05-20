using Microsoft.Extensions.Logging;
using AuthApp.Services;
using AuthApp.ViewModels;
using AuthApp.Views;
using CommunityToolkit.Maui;
using ZXing.Net.Maui.Controls;

namespace AuthApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Services
		builder.Services.AddSingleton<ITotpService, TotpService>();
		builder.Services.AddSingleton<IAccountStorageService, AccountStorageService>();
		builder.Services.AddSingleton<IIconService, IconService>();
		builder.Services.AddSingleton<IBackupService, BackupService>();

		// ViewModels
		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddTransient<AddAccountViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();
		builder.Services.AddTransient<WelcomeViewModel>();

		// Pages
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<WelcomePage>();
		builder.Services.AddTransient<QrScannerPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
