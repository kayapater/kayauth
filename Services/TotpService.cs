using OtpNet;

namespace AuthApp.Services;

public interface ITotpService
{
    string GenerateCode(string secretKey);
    double GetPreciseRemainingProgress();
    int GetRemainingSeconds();
}

public class TotpService : ITotpService
{
    public string GenerateCode(string secretKey)
    {
        try
        {
            var bytes = Base32Encoding.ToBytes(secretKey.Replace(" ", ""));
            var totp = new Totp(bytes);
            return totp.ComputeTotp();
        }
        catch
        {
            return "000 000";
        }
    }

    public double GetPreciseRemainingProgress()
    {
        var unixTimestampMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var remainingMillis = 30000 - (unixTimestampMillis % 30000);
        return remainingMillis / 30000.0;
    }

    public int GetRemainingSeconds()
    {
        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return (int)(30 - (unixTimestamp % 30));
    }
}
