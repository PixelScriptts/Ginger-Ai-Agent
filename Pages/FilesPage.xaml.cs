namespace AiAgent.Pages;

public partial class FilesPage : ContentPage
{
    // Fields referenced from XAML (will be wired by LoadFromXaml)
    private Label FileLabel;
    private Label PreviewLabel;

    public FilesPage()
    {
        InitializeComponent();
    }

    // Fallback InitializeComponent that loads XAML at runtime when source-gen isn't available
    public void InitializeComponent()
    {
        Microsoft.Maui.Controls.Xaml.Extensions.LoadFromXaml(this, typeof(FilesPage));
    }

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
