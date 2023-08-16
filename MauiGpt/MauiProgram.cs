using Microsoft.Extensions.Logging;
using MauiGpt.Data;

namespace MauiGpt;

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
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<OpenAiService>();
        builder.Services.AddSingleton<MarkdownToHtml>();
        builder.Services.AddSingleton<ChatMessages>();

        return builder.Build();
	}
}
