# ── 1. Patch BuildForce.csproj ────────────────────────────────────────────────
csproj_path = "BuildForce.csproj"

with open(csproj_path, "r", encoding="utf-8") as f:
    content = f.read()

content = content.replace(
    "<ApplicationId>com.companyname.buildforce</ApplicationId>",
    "<ApplicationId>com.mezano.buildforce</ApplicationId>"
)

with open(csproj_path, "w", encoding="utf-8") as f:
    f.write(content)

print("[OK] Patched ApplicationId -> com.mezano.buildforce")

# ── 2. Write MauiProgram.cs ───────────────────────────────────────────────────
maui_program = '#pragma warning disable CA1416\nusing CommunityToolkit.Maui;\nusing Microsoft.Extensions.Logging;\nusing BuildForce.Services;\nusing BuildForce.Views;\n\nnamespace BuildForce;\n\npublic static class MauiProgram\n{\n    public static MauiApp CreateMauiApp()\n    {\n        var builder = MauiApp.CreateBuilder();\n        builder\n            .UseMauiApp<App>()\n            .UseMauiCommunityToolkit()\n            .ConfigureFonts(fonts =>\n            {\n                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");\n                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");\n            });\n\n        // HTTP Client\n        builder.Services.AddSingleton<HttpClient>(sp => new HttpClient\n        {\n            BaseAddress = new Uri("https://mezanoconstructionmanagementplatform.com"),\n            Timeout = TimeSpan.FromSeconds(30)\n        });\n\n        // Services\n        builder.Services.AddSingleton<AuthService>();\n        builder.Services.AddSingleton<ApiService>();\n\n        // Pages\n        builder.Services.AddTransient<LoginPage>();\n        builder.Services.AddTransient<MainShellPage>();\n        builder.Services.AddTransient<DashboardPage>();\n        builder.Services.AddTransient<InvoicesPage>();\n        builder.Services.AddTransient<ProjectsPage>();\n        builder.Services.AddTransient<TimeClockPage>();\n\n#if DEBUG\n        builder.Logging.AddDebug();\n#endif\n\n        return builder.Build();\n    }\n}\n'

with open("MauiProgram.cs", "w", encoding="utf-8") as f:
    f.write(maui_program)

print("[OK] Wrote MauiProgram.cs with HttpClient registered")
print()
print("Next steps:")
print("  1. Visual Studio -> Build -> Clean Solution")
print("  2. Build -> Rebuild Solution")
print("  3. Deploy to Samsung")