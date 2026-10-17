namespace AiAgent.Pages;

public partial class FilesPage : ContentPage
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
