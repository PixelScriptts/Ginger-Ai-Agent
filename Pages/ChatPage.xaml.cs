using AiAgent.Models;
using AiAgent.Services;
using System.Collections.ObjectModel;

namespace AiAgent.Pages;

public partial class ChatPage : ContentPage
{
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = RunEnterAnimation();
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
{
    private readonly ConversationStore _store;
    private readonly AgentService _agent;
    private ObservableCollection<ChatMessage> _messages = new();
    private Conversation _current = new();

    public ChatPage(ConversationStore store, AgentService agent)
    {
        InitializeComponent();
        _store = store;
        _agent = agent;
        MessagesView.ItemsSource = _messages;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var list = await _store.LoadConversationsAsync();
        if (list.Count == 0)
        {
            _current = new Conversation { Title = "New conversation" };
        }
        else
        {
            _current = list[0];
            _messages = new ObservableCollection<ChatMessage>(_current.Messages);
            MessagesView.ItemsSource = _messages;
        }
    }

    private async void OnSendClicked(object? sender, EventArgs e)
    {
        var text = PromptEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text) || Busy.IsRunning) return;

        var user = new ChatMessage { Role = "user", Content = text };
        _messages.Add(user);
        _current.Messages.Add(user);
        if (_current.Messages.Count == 1) _current.Title = text.Length > 40 ? text[..40] : text;
        PromptEntry.Text = "";
        MessagesView.ScrollTo(_messages.Count - 1, position: ScrollToPosition.End, animate: true);

        Busy.IsVisible = true; Busy.IsRunning = true;
        try
        {
            var reply = await _agent.SendAsync(text);
            var bot = new ChatMessage { Role = "assistant", Content = reply };
            _messages.Add(bot);
            _current.Messages.Add(bot);
            MessagesView.ScrollTo(_messages.Count - 1, position: ScrollToPosition.End, animate: true);
            await PersistAsync();
        }
        catch (Exception ex)
        {
            var err = new ChatMessage { Role = "assistant", Content = ex.Message };
            _messages.Add(err);
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            Busy.IsVisible = false; Busy.IsRunning = false;
        }
    }

    private async Task PersistAsync()
    {
        var list = await _store.LoadConversationsAsync();
        var idx = list.FindIndex(c => c.Id == _current.Id);
        if (idx >= 0) list[idx] = _current; else list.Insert(0, _current);
        _current.UpdatedAt = DateTime.UtcNow;
        await _store.SaveConversationsAsync(list);
    }
}
