using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AuthApp.Models;
using AuthApp.Services;
using OtpNet;
using System.Web;

namespace AuthApp.ViewModels;

[QueryProperty(nameof(QrCodeResult), "qrCode")]
public partial class AddAccountViewModel : ObservableObject
{
    private readonly IAccountStorageService _storageService;
    private readonly IIconService _iconService;
    private readonly ITotpService _totpService;
    private IDispatcherTimer? _timer;
    private CancellationTokenSource? _iconUpdateCts;

    [ObservableProperty]
    private string _issuer = string.Empty;

    [ObservableProperty]
    private string _accountName = string.Empty;

    [ObservableProperty]
    private string _secretKey = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string? _previewIconUrl = "dotnet_bot.png";

    [ObservableProperty]
    private string _previewCode = "--- ---";

    [ObservableProperty]
    private double _previewRemainingProgress = 1.0;

    [ObservableProperty]
    private bool _isPreviewVisible;

    public event EventHandler? OnRequestClose;

    private string? _qrCodeResult;
    public string? QrCodeResult 
    { 
        get => _qrCodeResult; 
        set 
        {
            _qrCodeResult = value;
            if (!string.IsNullOrEmpty(value))
            {
                ParseOtpAuthUrl(value);
            }
        }
    }

    public AddAccountViewModel(IAccountStorageService storageService, IIconService iconService, ITotpService totpService)
    {
        _storageService = storageService;
        _iconService = iconService;
        _totpService = totpService;
        
        StartTimer();
    }

    private void StartTimer()
    {
        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer != null)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(200);
            _timer.Tick += (s, e) => UpdatePreview();
            _timer.Start();
        }
    }

    private void UpdatePreview()
    {
        if (string.IsNullOrWhiteSpace(SecretKey) || SecretKey.Length < 8)
        {
            IsPreviewVisible = false;
            PreviewCode = "--- ---";
            return;
        }

        try
        {
            var cleanKey = SecretKey.Replace(" ", "");
            Base32Encoding.ToBytes(cleanKey);
            
            var code = _totpService.GenerateCode(cleanKey);
            if (code.Length == 6)
            {
                PreviewCode = $"{code.Substring(0, 3)} {code.Substring(3, 3)}";
            }
            else
            {
                PreviewCode = code;
            }
            PreviewRemainingProgress = _totpService.GetPreciseRemainingProgress();
            IsPreviewVisible = true;
        }
        catch
        {
            IsPreviewVisible = false;
            PreviewCode = "--- ---";
        }
    }

    private void RequestClose()
    {
        _timer?.Stop();
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void ParseOtpAuthUrl(string url)
    {
        try
        {
            url = HttpUtility.UrlDecode(url);
            if (!url.StartsWith("otpauth://", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Geçersiz QR Kod formatı.";
                return;
            }

            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            
            string secret = query["secret"] ?? "";
            string queryIssuer = query["issuer"] ?? "";

            // Path parsing: /totp/Google:user@email.com or /totp/Google
            string path = uri.LocalPath.TrimStart('/');
            string label = path.Contains('/') ? path.Substring(path.IndexOf('/') + 1) : path;
            
            string detectedIssuer = "";
            string detectedAccount = "";

            if (label.Contains(':'))
            {
                var parts = label.Split(':');
                detectedIssuer = parts[0];
                detectedAccount = parts[1];
            }
            else
            {
                detectedIssuer = label;
            }

            // Prioritize query issuer if exists
            Issuer = !string.IsNullOrEmpty(queryIssuer) ? queryIssuer : detectedIssuer;
            AccountName = detectedAccount;

            if (!string.IsNullOrEmpty(secret))
            {
                SecretKey = secret.Replace(" ", "").ToUpper();
                ErrorMessage = "";
            }
            
            RefreshIcon();
        }
        catch (Exception ex)
        {
            ErrorMessage = "QR Kod ayrıştırılamadı: " + ex.Message;
        }
    }

    [RelayCommand]
    private void RefreshIcon()
    {
        if (!string.IsNullOrWhiteSpace(Issuer))
        {
            PreviewIconUrl = null;
            PreviewIconUrl = _iconService.GetIconUrl(Issuer);
        }
    }

    partial void OnIssuerChanged(string value)
    {
        _iconUpdateCts?.Cancel();
        _iconUpdateCts = new CancellationTokenSource();
        var token = _iconUpdateCts.Token;

        Task.Run(async () => {
            try 
            {
                await Task.Delay(500, token);
                if (string.IsNullOrWhiteSpace(value))
                    PreviewIconUrl = "dotnet_bot.png";
                else
                    PreviewIconUrl = _iconService.GetIconUrl(value);
            }
            catch (OperationCanceledException) { }
        });
    }

    [RelayCommand]
    private async Task ScanQr()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }

        if (status == PermissionStatus.Granted)
        {
            // Close popup first then navigate
            RequestClose();
            await Shell.Current.GoToAsync("QrScannerPage");
        }
        else
        {
            await Application.Current!.MainPage!.DisplayAlert("İzin Gerekli", "QR taramak için kamera izni vermeniz gerekiyor.", "Tamam");
        }
    }

    [RelayCommand]
    private async Task SaveAccount()
    {
        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(SecretKey))
        {
            ErrorMessage = "Lütfen Sağlayıcı ve Gizli Anahtar alanlarını doldurun.";
            return;
        }

        try
        {
            Base32Encoding.ToBytes(SecretKey);
            
            var newAccount = new AuthenticatorAccount
            {
                Issuer = Issuer,
                AccountName = AccountName,
                SecretKey = SecretKey,
                IconUrl = PreviewIconUrl ?? "dotnet_bot.png"
            };

            await _storageService.AddAccountAsync(newAccount);
            RequestClose();
        }
        catch (Exception)
        {
            ErrorMessage = "Geçersiz Gizli Anahtar formatı!";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose();
    }
}
