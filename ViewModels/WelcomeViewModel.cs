using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AuthApp.Services;

namespace AuthApp.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{
    private readonly IBackupService _backupService;
    private readonly IAccountStorageService _storageService;

    public WelcomeViewModel(IBackupService backupService, IAccountStorageService storageService)
    {
        _backupService = backupService;
        _storageService = storageService;
    }

    [RelayCommand]
    private async Task StartFresh()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    [RelayCommand]
    private async Task RestoreBackup()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Yedek Dosyasını Seçin" });
        if (result == null) return;

        string password = await Application.Current!.MainPage!.DisplayPromptAsync("Güvenlik", "Yedek dosyasının şifresini girin:", "Tamam", "Vazgeç");
        if (string.IsNullOrEmpty(password)) return;

        var accounts = await _backupService.ImportBackupAsync(password, result.FullPath);
        if (accounts != null)
        {
            await _storageService.SaveAccountsAsync(accounts);
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await Application.Current!.MainPage!.DisplayAlert("Hata", "Yedek yüklenemedi. Şifre yanlış olabilir.", "Tamam");
        }
    }
}
