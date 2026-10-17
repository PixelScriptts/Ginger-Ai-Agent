using AiAgent.Models;
using AiAgent.Services;

namespace AiAgent.Pages;

public partial class SettingsPage : BaseAnimatedPage
{
    private readonly ConversationStore _store;
    private AppSettings _settings = new();

    public SettingsPage(ConversationStore store)
    {
        InitializeComponent();
        _store = store;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _settings = _store.LoadSettings();

        ProviderPicker.ItemsSource = Providers.All.Select(p => p.Name).ToList();
        var idx = Providers.All.ToList().FindIndex(p => p.Id == _settings.SelectedProviderId);
        ProviderPicker.SelectedIndex = idx >= 0 ? idx : 0;

        UseCustomSwitch.IsToggled = _settings.UseCustomProvider;
        CustomProviderIdEntry.Text = _settings.CustomProviderId;
        CustomProviderBaseUrlEntry.Text = _settings.CustomProviderBaseUrl;
        CustomIdLabel.IsVisible = _settings.UseCustomProvider;
        CustomProviderIdEntry.IsVisible = _settings.UseCustomProvider;
        CustomBaseUrlLabel.IsVisible = _settings.UseCustomProvider;
        CustomProviderBaseUrlEntry.IsVisible = _settings.UseCustomProvider;

        ModelEntry.Text = _settings.SelectedModel;
        SystemPromptEditor.Text = _settings.SystemPrompt;
        TempSlider.Value = _settings.Temperature;
        TempLabel.Text = _settings.Temperature.ToString("0.00");
        MaxTokensEntry.Text = _settings.MaxTokens.ToString();

        await RefreshKeyStatusAsync();
        await RefreshConnectionsAsync();
    }

    private async void OnProviderChanged(object? sender, EventArgs e)
    {
        if (ProviderPicker.SelectedIndex < 0) return;
        _settings.SelectedProviderId = Providers.All[ProviderPicker.SelectedIndex].Id;
        _store.SaveSettings(_settings);
        await RefreshKeyStatusAsync();
    }

    private void OnUseCustomToggled(object? sender, ToggledEventArgs e)
    {
        var on = e.Value;
        CustomIdLabel.IsVisible = on;
        CustomProviderIdEntry.IsVisible = on;
        CustomBaseUrlLabel.IsVisible = on;
        CustomProviderBaseUrlEntry.IsVisible = on;
        _settings.UseCustomProvider = on;
        _store.SaveSettings(_settings);
    }

    private async Task RefreshKeyStatusAsync()
    {
        var id = GetEffectiveProviderId();
        var info = Providers.All.First(p => p.Id == id);
        var key = await ProviderCredentialService.GetAsync(id);
        if (!string.IsNullOrWhiteSpace(key))
        {
            KeyStatusLabel.Text = $"Stored securely on this device ({Mask(key)})";
            KeyStatusLabel.TextColor = Color.FromArgb("#6CFF8A");
            ApiKeyEntry.Placeholder = Mask(key);
        }
        else
        {
            KeyStatusLabel.Text = $"No key saved for {info.Name}. Paste key and tap Save.";
            KeyStatusLabel.TextColor = Color.FromArgb("#FF6B6B");
            ApiKeyEntry.Placeholder = info.Placeholder;
        }
    }

    private static string Mask(string k) => k.Length <= 8 ? "****" : k[..4] + new string('*', Math.Min(12, k.Length - 8)) + k[^4..];

    private async void OnSaveKey(object? sender, EventArgs e)
    {
        var raw = ApiKeyEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            SaveResultLabel.TextColor = Color.FromArgb("#FF6B6B");
            SaveResultLabel.Text = "Enter an API key before saving.";
            return;
        }

        var id = GetEffectiveProviderId();
        SaveResultLabel.TextColor = Color.FromArgb("#FFD66C");
        SaveResultLabel.Text = "Saving...";
        try
        {
            var ok = await ProviderCredentialService.SaveAsync(id, raw);
            await RefreshKeyStatusAsync();
            ApiKeyEntry.Text = "";
            if (ok)
            {
                SaveResultLabel.TextColor = Color.FromArgb("#6CFF8A");
                SaveResultLabel.Text = "Saved to secure store. Provider connection is ready.";
            }
            else
            {
                SaveResultLabel.TextColor = Color.FromArgb("#FF6B6B");
                SaveResultLabel.Text = "Save returned false. Try again or check device keystore.";
            }
            await RefreshConnectionsAsync();
        }
        catch (Exception ex)
        {
            SaveResultLabel.TextColor = Color.FromArgb("#FF6B6B");
            SaveResultLabel.Text = ex.Message;
        }
    }

    private async void OnDeleteKey(object? sender, EventArgs e)
    {
        var id = GetEffectiveProviderId();
        ProviderCredentialService.Remove(id);
        SaveResultLabel.TextColor = Color.FromArgb("#FFD66C");
        SaveResultLabel.Text = "Key removed.";
        await RefreshKeyStatusAsync();
        await RefreshConnectionsAsync();
    }

    private async void OnTestKey(object? sender, EventArgs e)
    {
        var id = GetEffectiveProviderId();
        var key = await ProviderCredentialService.GetAsync(id);
        if (string.IsNullOrWhiteSpace(key))
        {
            SaveResultLabel.TextColor = Color.FromArgb("#FF6B6B");
            SaveResultLabel.Text = "No key stored to test. Save a key first.";
            return;
        }
        SaveResultLabel.TextColor = Color.FromArgb("#FFD66C");
        SaveResultLabel.Text = "Key is present in secure store.";
    }

    // Ensure we always have a valid provider id to use. Prefer the settings value, then the picker selection, then the first known provider.
    private string GetEffectiveProviderId()
    {
        // If using a custom provider, prefer the custom id
        if (_settings != null && _settings.UseCustomProvider && !string.IsNullOrWhiteSpace(_settings.CustomProviderId))
            return _settings.CustomProviderId.Trim();
        if (!string.IsNullOrWhiteSpace(_settings?.SelectedProviderId) && Providers.All.Any(p => p.Id == _settings.SelectedProviderId))
            return _settings.SelectedProviderId;
        if (ProviderPicker.SelectedIndex >= 0 && ProviderPicker.SelectedIndex < Providers.All.Count)
        {
            var id = Providers.All[ProviderPicker.SelectedIndex].Id;
            _settings.SelectedProviderId = id;
            _store.SaveSettings(_settings);
            return id;
        }
        // fallback
        var fallback = Providers.All.First().Id;
        _settings.SelectedProviderId = fallback;
        _store.SaveSettings(_settings);
        return fallback;
    }

    private void OnTempChanged(object? sender, ValueChangedEventArgs e)
    {
        TempLabel.Text = e.NewValue.ToString("0.00");
    }

    private async void OnSaveSettings(object? sender, EventArgs e)
    {
        _settings.SelectedModel = ModelEntry.Text?.Trim() ?? "";
        _settings.SystemPrompt = SystemPromptEditor.Text ?? "";
        _settings.Temperature = TempSlider.Value;
        _settings.UseCustomProvider = UseCustomSwitch.IsToggled;
        _settings.CustomProviderId = CustomProviderIdEntry.Text?.Trim() ?? "";
        _settings.CustomProviderBaseUrl = CustomProviderBaseUrlEntry.Text?.Trim() ?? "";
        if (int.TryParse(MaxTokensEntry.Text, out var mt)) _settings.MaxTokens = mt;
        _store.SaveSettings(_settings);
        SaveResultLabel.TextColor = Color.FromArgb("#6CFF8A");
        SaveResultLabel.Text = "Settings saved.";
        await RefreshConnectionsAsync();
    }

    private async Task RefreshConnectionsAsync()
    {
        var ids = Providers.All.Select(p => p.Id).ToList();
        if (_settings.UseCustomProvider && !string.IsNullOrWhiteSpace(_settings.CustomProviderId)) ids.Add(_settings.CustomProviderId);
        var status = await ProviderCredentialService.GetConnectionStatusAsync(ids);
        var list = Providers.All.Select(p => new
        {
            Name = p.Name,
            Connected = status.TryGetValue(p.Id, out var v) && v,
            Status = status.TryGetValue(p.Id, out var v2) && v2 ? "Connected" : "Not set",
        }).ToList();
        if (_settings.UseCustomProvider && !string.IsNullOrWhiteSpace(_settings.CustomProviderId))
        {
            list.Add(new { Name = "Custom", Connected = status.TryGetValue(_settings.CustomProviderId, out var cv) && cv, Status = status.TryGetValue(_settings.CustomProviderId, out var cv2) && cv2 ? "Connected" : "Not set" });
        }
        ConnectionList.ItemsSource = list;
    }
}
