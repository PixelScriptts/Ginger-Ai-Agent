namespace AiAgent.Pages;

public partial class TerminalPage : ContentPage
{
    public TerminalPage() => InitializeComponent();

    private async void OnRun(object? sender, EventArgs e)
    {
        var cmd = CmdEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(cmd)) return;
        Output.Text += $"\n$ {cmd}\n";
        CmdEntry.Text = "";
        // Minimal in-app interpreter (no process launch for safety/cross-platform)
        try
        {
            string result = cmd switch
            {
                var s when s.StartsWith("echo ") => s[5..],
                "pwd" => FileSystem.AppDataDirectory,
                "date" => DateTime.Now.ToString("O"),
                "help" => "Supported: echo <text>, pwd, date, clear, help",
                "clear" => "",
                _ => $"Unknown command: {cmd}  (try: help)",
            };
            if (cmd == "clear") Output.Text = "Ginger Terminal — sandboxed.\n";
            else Output.Text += result + "\n";
        }
        catch (Exception ex) { Output.Text += ex.Message + "\n"; await Task.CompletedTask; }
    }
}
