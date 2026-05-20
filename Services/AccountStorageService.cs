using System.Text.Json;
using AuthApp.Models;

namespace AuthApp.Services;

public interface IAccountStorageService
{
    Task<List<AuthenticatorAccount>> GetAccountsAsync();
    Task SaveAccountsAsync(List<AuthenticatorAccount> accounts);
    Task AddAccountAsync(AuthenticatorAccount account);
}

public class AccountStorageService : IAccountStorageService
{
    private const string StorageKey = "authenticator_accounts";

    public async Task<List<AuthenticatorAccount>> GetAccountsAsync()
    {
        var json = await SecureStorage.Default.GetAsync(StorageKey);
        if (string.IsNullOrEmpty(json))
            return new List<AuthenticatorAccount>();

        return JsonSerializer.Deserialize<List<AuthenticatorAccount>>(json) ?? new List<AuthenticatorAccount>();
    }

    public async Task SaveAccountsAsync(List<AuthenticatorAccount> accounts)
    {
        var json = JsonSerializer.Serialize(accounts);
        await SecureStorage.Default.SetAsync(StorageKey, json);
    }

    public async Task AddAccountAsync(AuthenticatorAccount account)
    {
        var accounts = await GetAccountsAsync();
        accounts.Add(account);
        await SaveAccountsAsync(accounts);
    }
}
