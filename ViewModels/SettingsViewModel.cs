using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AuthApp.Services;
using CommunityToolkit.Maui.Storage;
using System.Text;
using System.Text.Json;

namespace AuthApp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IBackupService _backupService;
    private readonly IAccountStorageService _storageService;

    [ObservableProperty]
    private string _selectedTheme = Helpers.ThemeManager.DefaultTheme;

    public SettingsViewModel(IBackupService backupService, IAccountStorageService storageService)
    {
        _backupService = backupService;
        _storageService = storageService;
        SelectedTheme = Preferences.Get("selected_theme", Helpers.ThemeManager.DefaultTheme);
    }

    [RelayCommand]
    private void SelectTheme(string themeName)
    {
        SelectedTheme = themeName;
        Helpers.ThemeManager.ApplyTheme(themeName);
    }

    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Backup()
    {
        var accounts = await _storageService.GetAccountsAsync();
        if (accounts.Count == 0)
        {
            await Application.Current!.MainPage!.DisplayAlert("Yedekle", "Yedeklenecek hesap bulunamadı.", "Tamam");
            return;
        }

        string password = await Application.Current!.MainPage!.DisplayPromptAsync("Güvenlik", "Yedek dosyası için bir şifre belirleyin:", "Tamam", "Vazgeç");
        if (string.IsNullOrEmpty(password)) return;

        try
        {
            var fileSaverResult = await FileSaver.Default.SaveAsync("authenticator_backup.auth", new MemoryStream(Encoding.UTF8.GetBytes("temp")), default);
            
            if (fileSaverResult.IsSuccessful)
            {
                bool success = await _backupService.ExportBackupAsync(accounts, password, fileSaverResult.FilePath);
                if (success)
                    await Application.Current!.MainPage!.DisplayAlert("Başarılı", "Yedek başarıyla kaydedildi.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Hata", $"Yedekleme sırasında bir sorun oluştu: {ex.Message}", "Tamam");
        }
    }

    [RelayCommand]
    private async Task Restore()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions 
        { 
            PickerTitle = "Yedek Dosyasını Seçin",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".auth" } }
            })
        });
        
        if (result == null) return;

        string password = await Application.Current!.MainPage!.DisplayPromptAsync("Güvenlik", "Yedek dosyasının şifresini girin:", "Tamam", "Vazgeç");
        if (string.IsNullOrEmpty(password)) return;

        var accounts = await _backupService.ImportBackupAsync(password, result.FullPath);
        if (accounts != null)
        {
            await _storageService.SaveAccountsAsync(accounts);
            await Application.Current!.MainPage!.DisplayAlert("Başarılı", "Yedek başarıyla geri yüklendi.", "Tamam");
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await Application.Current!.MainPage!.DisplayAlert("Hata", "Yedek yüklenemedi. Şifre yanlış olabilir.", "Tamam");
        }
    }
}
