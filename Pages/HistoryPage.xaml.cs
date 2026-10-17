using AiAgent.Services;

namespace AiAgent.Pages;

public partial class HistoryPage : ContentPage
{
    private readonly ConversationStore _store;
    public HistoryPage(ConversationStore store) { InitializeComponent(); _store = store; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        List.ItemsSource = (await _store.LoadConversationsAsync()).OrderByDescending(c => c.UpdatedAt).ToList();
    }

    private async void OnClear(object? sender, EventArgs e)
    {
        if (!await DisplayAlert("Clear history", "Delete all conversations?", "Delete", "Cancel")) return;
        await _store.SaveConversationsAsync(new());
        List.ItemsSource = new List<object>();
    }
}
