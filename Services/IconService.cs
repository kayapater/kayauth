namespace AuthApp.Services;

public interface IIconService
{
    string GetIconUrl(string issuer);
}

public class IconService : IIconService
{
    public string GetIconUrl(string issuer)
    {
        if (string.IsNullOrWhiteSpace(issuer))
            return "dotnet_bot.png";

        string cleanIssuer = issuer.Trim().ToLower();
        
        // Comprehensive domain mapping for top services
        string domain = cleanIssuer switch
        {
            "google" or "gmail" => "google.com",
            "github" => "github.com",
            "facebook" => "facebook.com",
            "microsoft" or "outlook" or "hotmail" => "microsoft.com",
            "binance" => "binance.com",
            "discord" => "discord.com",
            "twitter" or "x" => "twitter.com",
            "instagram" => "instagram.com",
            "apple" or "icloud" => "apple.com",
            "slack" => "slack.com",
            "netflix" => "netflix.com",
            "spotify" => "spotify.com",
            "twitch" => "twitch.tv",
            "dropbox" => "dropbox.com",
            "gitlab" => "gitlab.com",
            "bitbucket" => "bitbucket.org",
            "amazon" or "aws" => "amazon.com",
            "digitalocean" => "digitalocean.com",
            "cloudflare" => "cloudflare.com",
            "coinbase" => "coinbase.com",
            "kraken" => "coinbase.com",
            "epic games" => "epicgames.com",
            "steam" => "steampowered.com",
            _ => $"{cleanIssuer.Replace(" ", "")}.com"
        };

        // IconHorse is often more reliable for various brand logos and returns a PNG
        return $"https://icon.horse/icon/{domain}";
    }
}
