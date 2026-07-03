#pragma warning disable CA1416
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using BuildForce.Services;
using BuildForce.Views;
namespace BuildForce;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        // HTTP Client
        builder.Services.AddSingleton<HttpClient>(sp => new HttpClient
        {
            BaseAddress = new Uri("https://mezanocm.com"),
            Timeout = TimeSpan.FromSeconds(30)
        });
        // Services
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ApiService>();
        // App
        builder.Services.AddSingleton<App>();
        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainShellPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<InvoicesPage>();
        builder.Services.AddTransient<ProjectsPage>();
        builder.Services.AddTransient<TimeClockPage>();
        builder.Services.AddTransient<TimesheetDetailPage>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}