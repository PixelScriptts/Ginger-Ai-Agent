using Microsoft.Extensions.Logging;
using AiAgent.Services;
using AiAgent.Pages;

namespace AiAgent;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<ConversationStore>();
		builder.Services.AddSingleton<AgentService>();
		builder.Services.AddTransient<ChatPage>();
		builder.Services.AddTransient<AgentPage>();
		builder.Services.AddTransient<FilesPage>();
		builder.Services.AddTransient<TerminalPage>();
		builder.Services.AddTransient<PreviewPage>();
		builder.Services.AddTransient<HistoryPage>();
		builder.Services.AddTransient<SettingsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
