namespace AiAgent.Pages;

public partial class PreviewPage : ContentPage
{
    public PreviewPage() => InitializeComponent();

    private void OnLoad(object? sender, EventArgs e)
    {
        var url = UrlEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(url)) return;
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("file", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;
        Viewer.Source = url;
    }
}
