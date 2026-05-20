using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AuthApp.Models;
using AuthApp.Services;

namespace AuthApp.ViewModels;

[QueryProperty(nameof(QrCode), "qrCode")]
public partial class MainViewModel : ObservableObject
{
    private readonly ITotpService _totpService;
    private readonly IAccountStorageService _storageService;
    private readonly IIconService _iconService;
    private IDispatcherTimer? _timer;
    private List<AuthenticatorAccount> _allAccounts = new();

    [ObservableProperty]
    private string? _qrCode;

    partial void OnQrCodeChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            // Open the add account popup
            NavigateToAddAccount();
            
            // Clear code so it doesn't trigger again on re-appear
            QrCode = null;
        }
    }

    [ObservableProperty]
    private ObservableCollection<AuthenticatorAccount> _filteredAccounts = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isCopyMessageVisible;

    [ObservableProperty]
    private bool _isEmptyStateVisible;

    [ObservableProperty]
    private bool _isCompactMode;

    public MainViewModel(ITotpService totpService, IAccountStorageService storageService, IIconService iconService)
    {
        _totpService = totpService;
        _storageService = storageService;
        _iconService = iconService;
        
        // Default mode
        IsCompactMode = false;
        
        StartTimer();
    }

    [RelayCommand]
    private void ToggleViewMode()
    {
        IsCompactMode = !IsCompactMode;
    }

    public async Task InitializeAsync()
    {
        _allAccounts = await _storageService.GetAccountsAsync();
        
        // Ensure icons are set
        foreach (var account in _allAccounts)
        {
            if (string.IsNullOrEmpty(account.IconUrl))
            {
                account.IconUrl = _iconService.GetIconUrl(account.Issuer);
            }
        }

        FilterAccounts();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterAccounts();
    }

    private void FilterAccounts()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allAccounts
            : _allAccounts.Where(a => 
                a.Issuer.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                a.AccountName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

        FilteredAccounts.Clear();
        foreach (var account in filtered)
        {
            FilteredAccounts.Add(account);
        }
        
        IsEmptyStateVisible = !FilteredAccounts.Any();
        UpdateCodes();
    }

    private void StartTimer()
    {
        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer != null)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += (s, e) => UpdateCodes();
            _timer.Start();
        }
    }

    private void UpdateCodes()
    {
        var progress = _totpService.GetPreciseRemainingProgress();

        foreach (var account in FilteredAccounts)
        {
            account.CurrentCode = _totpService.GenerateCode(account.SecretKey);
            account.RemainingProgress = progress;
        }
    }

    [RelayCommand]
    private async Task NavigateToSettings()
    {
        await Shell.Current.GoToAsync("SettingsPage");
    }

    public event EventHandler? OnRequestAddAccount;

    [RelayCommand]
    private void NavigateToAddAccount()
    {
        OnRequestAddAccount?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task CopyToClipboard(string? code)
    {
        if (string.IsNullOrEmpty(code)) return;
        await Clipboard.Default.SetTextAsync(code);
        
        IsCopyMessageVisible = true;
        await Task.Delay(2000);
        IsCopyMessageVisible = false;
    }

    [RelayCommand]
    private async Task DeleteAccount(AuthenticatorAccount? account)
    {
        if (account is null)
            return;

        var hostPage = Shell.Current ?? Application.Current?.MainPage;
        if (hostPage is null)
            return;

        bool answer = await hostPage.DisplayAlert(
            "Sil",
            $"{account.Issuer} hesabını silmek istediğinize emin misiniz?",
            "Evet",
            "Hayır");

        if (!answer)
            return;

        await DeleteAccountAsync(account);
    }

    public async Task DeleteAccountAsync(AuthenticatorAccount account)
    {
        if (account is null)
            return;

        var removed = _allAccounts.RemoveAll(a => a.Id == account.Id);
        if (removed == 0)
            return;

        await _storageService.SaveAccountsAsync(_allAccounts);
        FilterAccounts();
    }
}
