using CommunityToolkit.Mvvm.ComponentModel;

namespace AuthApp.Models;

public partial class AuthenticatorAccount : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Issuer { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    [ObservableProperty]
    private string? _iconUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedCode))]
    private string? _currentCode;

    [ObservableProperty]
    private double _remainingProgress;

    public string FormattedCode 
    {
        get 
        {
            if (string.IsNullOrEmpty(CurrentCode) || CurrentCode.Length != 6)
                return CurrentCode ?? "000 000";
            
            return $"{CurrentCode.Substring(0, 3)} {CurrentCode.Substring(3, 3)}";
        }
    }
}
