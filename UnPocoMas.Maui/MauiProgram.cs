using UnPocoMas.Core.Services;
using Microsoft.Extensions.Logging;
using UnPocoMas.Maui.Services;
using UnPocoMas.UI.Interfaces;
using UnPocoMas.UI.Services;
using Microsoft.Maui.LifecycleEvents;

namespace UnPocoMas.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		var lifecycleService = new MauiAppLifecycleService();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			})
			.ConfigureLifecycleEvents(events =>
			{
#if WINDOWS
				events.AddWindows(window =>
				{
					window.OnVisibilityChanged((_, args) =>
					{
						if (args.Visible)
						{
							lifecycleService.NotifyResumed();
						}
						else
						{
							lifecycleService.NotifyBackgrounded();
						}
					});
				});
#endif
#if ANDROID
				events.AddAndroid(android =>
				{
					android.OnPause(activity =>
						lifecycleService.NotifyBackgrounded());
					android.OnResume(activity =>
						lifecycleService.NotifyResumed());
				});
#endif
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton<ChallengeGenerator>();
		builder.Services.AddSingleton<ScoreCalculator>();
		builder.Services.AddSingleton<ShareTextBuilder>();
		builder.Services.AddSingleton<IUtcDateProvider, SystemUtcDateProvider>();
		builder.Services.AddSingleton<IStorageService, MauiStorageService>();
		builder.Services.AddSingleton(lifecycleService);
		builder.Services.AddSingleton<IAppLifecycleService>(lifecycleService);
		builder.Services.AddSingleton<DailyGameService>();
		builder.Services.AddTransient<RoundRuntimeService>();
		builder.Services.AddSingleton<IShareService, DummyShareService>();
		builder.Services.AddSingleton<IHapticsService, DummyHapticsService>();
		builder.Services.AddSingleton<ISoundService, DummySoundService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static IServiceProvider CurrentServices { get; set; } = null!;
}
