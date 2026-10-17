namespace AiAgent.Pages;

public partial class FilesPage : ContentPage
{
    public FilesPage() => InitializeComponent();

    private async void OnPickFile(object? sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.PickAsync();
            if (file == null) return;
            FileLabel.Text = file.FileName + "  (" + file.FullPath + ")";
            try
            {
                var text = await File.ReadAllTextAsync(file.FullPath);
                PreviewLabel.Text = text.Length > 4000 ? text[..4000] + "\n..." : text;
            }
            catch { PreviewLabel.Text = "(Binary or unreadable file)"; }
        }
        catch (Exception ex) { await DisplayAlert("Files", ex.Message, "OK"); }
    }
}
