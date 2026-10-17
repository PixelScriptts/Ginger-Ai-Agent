using AiAgent.Services;

namespace AiAgent.Pages;

public partial class AgentPage : ContentPage
{

    private readonly ConversationStore _store;
    private readonly AgentService _agent;

    public AgentPage(ConversationStore store, AgentService agent)
    {
        InitializeComponent();
        _store = store;
        _agent = agent;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = RunEnterAnimation();
        var s = _store.LoadSettings();
        ProviderLabel.Text = $"Provider: {s.SelectedProviderId}  |  Model: {s.SelectedModel}";
    }

    private async Task RunEnterAnimation()
    {
        try
        {
            this.Opacity = 0;
            this.TranslationY = 12;
            await Task.WhenAll(this.FadeTo(1, 300, Easing.CubicOut), this.TranslateTo(0, 0, 300, Easing.CubicOut));
        }
        catch { }
    }

    private async void OnRun(object? sender, EventArgs e)
    {
        var task = TaskEditor.Text?.Trim();
        if (string.IsNullOrWhiteSpace(task) || Busy.IsRunning) return;
        Busy.IsVisible = true; Busy.IsRunning = true;
        OutputLabel.Text = "";
        try
        {
            var result = await _agent.SendAsync($"You are an autonomous assistant. Complete this task step by step:\n\n{task}");
            OutputLabel.Text = result;
        }
        catch (Exception ex)
        {
            OutputLabel.Text = ex.Message;
            await DisplayAlert("Agent", ex.Message, "OK");
        }
        finally { Busy.IsVisible = false; Busy.IsRunning = false; }
    }
}
