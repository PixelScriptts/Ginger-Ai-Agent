using System.Text.Json;
using AiAgent.Models;

namespace AiAgent.Services;

public class ConversationStore
{
    private const string ConversationsKey = "ginger.conversations.json";
    private const string SettingsKey = "ginger.settings.json";
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false, PropertyNameCaseInsensitive = true };

    public async Task<List<Conversation>> LoadConversationsAsync()
    {
        try
        {
            var json = await SecureStorage.GetAsync(ConversationsKey);
            // SecureStorage has small limit on some platforms; fall back to Preferences for large payloads
            if (string.IsNullOrEmpty(json))
                json = Preferences.Get(ConversationsKey, null);
            if (string.IsNullOrEmpty(json)) return new();
            return JsonSerializer.Deserialize<List<Conversation>>(json, JsonOpts) ?? new();
        }
        catch { return new(); }
    }

    public async Task SaveConversationsAsync(List<Conversation> list)
    {
        var json = JsonSerializer.Serialize(list, JsonOpts);
        // Prefer Preferences for large blob; SecureStorage is for secrets
        Preferences.Set(ConversationsKey, json);
        await Task.CompletedTask;
    }

    public AppSettings LoadSettings()
    {
        try
        {
            var json = Preferences.Get(SettingsKey, null);
            if (string.IsNullOrEmpty(json)) return new AppSettings();
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOpts) ?? new AppSettings();
        }
        catch { return new AppSettings(); }
    }

    public void SaveSettings(AppSettings s)
    {
        Preferences.Set(SettingsKey, JsonSerializer.Serialize(s, JsonOpts));
    }
}
