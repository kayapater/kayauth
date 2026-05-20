using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AuthApp.Models;

namespace AuthApp.Services;

public interface IBackupService
{
    Task<bool> ExportBackupAsync(List<AuthenticatorAccount> accounts, string password, string filePath);
    Task<List<AuthenticatorAccount>?> ImportBackupAsync(string password, string filePath);
}

public class BackupService : IBackupService
{
    private const int SaltSize = 16;
    private const int IvSize = 16;
    private const int Pbkdf2Iterations = 10000;

    public async Task<bool> ExportBackupAsync(List<AuthenticatorAccount> accounts, string password, string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(accounts);
            var encryptedData = EncryptPbkdf2(json, password);
            await File.WriteAllBytesAsync(filePath, encryptedData);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<AuthenticatorAccount>?> ImportBackupAsync(string password, string filePath)
    {
        try
        {
            var encryptedData = await File.ReadAllBytesAsync(filePath);
            
            // Try decrypting with new PBKDF2 format first
            try
            {
                var json = DecryptPbkdf2(encryptedData, password);
                var accounts = JsonSerializer.Deserialize<List<AuthenticatorAccount>>(json);
                if (accounts != null)
                    return accounts;
            }
            catch
            {
                // Decryption or deserialization failed. Fall back to legacy SHA256 decryption.
            }

            // Fallback to Legacy Decryption
            try
            {
                var jsonLegacy = DecryptLegacy(encryptedData, password);
                return JsonSerializer.Deserialize<List<AuthenticatorAccount>>(jsonLegacy);
            }
            catch
            {
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    private byte[] EncryptPbkdf2(string plainText, string password)
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(32); // AES-256 key size

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        var iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        ms.Write(salt, 0, salt.Length); // Write Salt (16 bytes)
        ms.Write(iv, 0, iv.Length);     // Write IV (16 bytes)

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        return ms.ToArray();
    }

    private string DecryptPbkdf2(byte[] cipherText, string password)
    {
        if (cipherText.Length < SaltSize + IvSize)
            throw new CryptographicException("Ciphertext is too short.");

        using var ms = new MemoryStream(cipherText);
        
        var salt = new byte[SaltSize];
        ms.Read(salt, 0, salt.Length);

        var iv = new byte[IvSize];
        ms.Read(iv, 0, iv.Length);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(32);

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }

    private string DecryptLegacy(byte[] cipherText, string password)
    {
        using Aes aes = Aes.Create();
        var key = DeriveKeyLegacy(password);
        aes.Key = key;

        using var ms = new MemoryStream(cipherText);
        var iv = new byte[16];
        ms.Read(iv, 0, iv.Length); // Read IV from start of file (legacy)
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }

    private byte[] DeriveKeyLegacy(string password)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(password));
    }
}
