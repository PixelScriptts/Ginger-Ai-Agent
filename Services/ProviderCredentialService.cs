namespace AiAgent.Services;

public static class ProviderCredentialService
{
    // Fixed: Expo SecureStore rejects ":" so original key "provider-credential:" failed silently.
    // We use only [A-Za-z0-9._-] which is accepted on Android Keystore + iOS Keychain + MAUI SecureStorage.
    private const string KeyPrefix = "provider_credential.";

    private static string KeyFor(string providerId)
    {
        if (string.IsNullOrWhiteSpace(providerId))
            throw new ArgumentException("Provider id is required.", nameof(providerId));

        // Sanitize to SecureStore-safe charset
        var lower = providerId.Trim().ToLowerInvariant();
        var sanitized = new string(lower.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_').ToArray());
        if (string.IsNullOrWhiteSpace(sanitized))
            sanitized = "default";
        return KeyPrefix + sanitized;
    }

    // Legacy key that the Replit APK used (contains colon -> always fails on SecureStore)
    private static string LegacyKeyFor(string providerId) => $"provider-credential:{providerId}";

    public static async Task<bool> SaveAsync(string providerId, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key must not be empty.", nameof(apiKey));

        var key = KeyFor(providerId);
        try
        {
            await SecureStorage.SetAsync(key, apiKey.Trim());
            // Clean up legacy key if it somehow exists
            try { SecureStorage.Remove(LegacyKeyFor(providerId)); } catch { }
            // Verify round-trip
            var verify = await SecureStorage.GetAsync(key);
            return verify == apiKey.Trim();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProviderCredentialService] Save failed for {providerId}: {ex.Message}");
            throw new InvalidOperationException($"Failed to save credential for '{providerId}': {ex.Message}", ex);
        }
    }

    public static async Task<string?> GetAsync(string providerId)
    {
        var key = KeyFor(providerId);
        try
        {
            var val = await SecureStorage.GetAsync(key);
            if (!string.IsNullOrEmpty(val))
                return val;
            // Fallback: try legacy key once (migration)
            try { return await SecureStorage.GetAsync(LegacyKeyFor(providerId)); } catch { return null; }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProviderCredentialService] Get failed for {providerId}: {ex.Message}");
            return null;
        }
    }

    public static bool Remove(string providerId)
    {
        try
        {
            SecureStorage.Remove(KeyFor(providerId));
            try { SecureStorage.Remove(LegacyKeyFor(providerId)); } catch { }
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProviderCredentialService] Remove failed for {providerId}: {ex.Message}");
            return false;
        }
    }

    public static async Task<Dictionary<string, bool>> GetConnectionStatusAsync(IEnumerable<string> providerIds)
    {
        var result = new Dictionary<string, bool>();
        foreach (var id in providerIds)
        {
            var key = await GetAsync(id);
            result[id] = !string.IsNullOrWhiteSpace(key);
        }
        return result;
    }
}
