import os

ROOT = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

# ── MauiProgram.cs ──────────────────────────────────────────
MP = os.path.join(ROOT, "MauiProgram.cs")
t = open(MP, encoding="utf-8").read()

# Add GroupClockInPage registration after NotificationsPage
t = t.replace(
    "builder.Services.AddTransient<NotificationsPage>();",
    "builder.Services.AddTransient<NotificationsPage>();\n        builder.Services.AddTransient<GroupClockInPage>();"
)
write(MP, t)

# ── AppShell.xaml ───────────────────────────────────────────
SHELL = os.path.join(ROOT, "AppShell.xaml")
t2 = open(SHELL, encoding="utf-8").read()

# Add GroupClockInPage route before </Shell>
t2 = t2.replace(
    "</Shell>",
    '    <Shell.Items>\n        <ShellContent Route="GroupClockInPage" ContentTemplate="{DataTemplate views:GroupClockInPage}" Shell.TabBarIsVisible="False" Shell.NavBarIsVisible="False"/>\n    </Shell.Items>\n</Shell>'
)
write(SHELL, t2)

print("\nDone! Build and deploy to Samsung.")