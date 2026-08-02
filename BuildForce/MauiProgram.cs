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
            .UseMauiCommunityToolkitCamera()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.Services.AddSingleton<HttpClient>(sp => new HttpClient
        {
            BaseAddress = new Uri("https://mezanocm.com"),
            Timeout = TimeSpan.FromSeconds(30)
        });
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<App>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainShellPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<InvoicesPage>();
        builder.Services.AddTransient<ProjectsPage>();
        builder.Services.AddTransient<TimeClockPage>();
        builder.Services.AddTransient<TimesheetDetailPage>();
        builder.Services.AddTransient<SafetyCheckPage>();
        builder.Services.AddTransient<InjuryReportPage>();
        builder.Services.AddTransient<ProjectCreatePage>();
        builder.Services.AddTransient<ExpenseCreatePage>();
        builder.Services.AddTransient<InvoiceCreatePage>();
        builder.Services.AddTransient<EstimateCreatePage>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        StripNativeUnderlines();
        WireSheetPickers();
        return builder.Build();
    }

    // [UL1] Android draws a Material underline under every EditText-backed control
    // (Entry, Editor, Picker, DatePicker, TimePicker, SearchBar) no matter what
    // BackgroundColor the XAML sets. Clearing it here does the whole app at once so
    // the Mezano rounded Border is the only visible chrome on every form.
    private static void StripNativeUnderlines()
    {
#if ANDROID
        static void Clear(object? platformView)
        {
            if (platformView is Android.Views.View v)
                v.SetBackgroundColor(Android.Graphics.Color.Transparent);
        }
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("MZNoUnderline", (h, v) => Clear(h.PlatformView));
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("MZNoUnderline", (h, v) => Clear(h.PlatformView));
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("MZNoUnderline", (h, v) => Clear(h.PlatformView));
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("MZNoUnderline", (h, v) => Clear(h.PlatformView));
        Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("MZNoUnderline", (h, v) => Clear(h.PlatformView));
        Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping("MZNoUnderline", (h, v) => Clear(h.PlatformView));
#endif
    }
    // [PSH3a] Route every MzPicker tap to the Mezano sheet instead of the native grey
    // AlertDialog. SetOnClickListener REPLACES the listener MAUI's PickerHandler installed
    // in ConnectHandler, which is what suppresses the native dialog. Plain Pickers are
    // left alone, so nothing outside the converted pages changes.
    private static void WireSheetPickers()
    {
#if ANDROID
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("MZSheetPicker", (h, v) =>
        {
            if (v is not BuildForce.Controls.MzPicker mp) return;
            if (h.PlatformView is not Android.Views.View av) return;
            av.Focusable = false;
            av.Clickable = true;
            av.SetOnClickListener(new MzPickerClickListener(mp));
        });
#endif
    }

#if ANDROID
    private sealed class MzPickerClickListener : Java.Lang.Object, Android.Views.View.IOnClickListener
    {
        private readonly BuildForce.Controls.MzPicker _picker;
        public MzPickerClickListener(BuildForce.Controls.MzPicker p) { _picker = p; }
        public void OnClick(Android.Views.View? v) { _ = _picker.ShowSheetAsync(); }
    }
#endif
}
