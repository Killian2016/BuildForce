import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

TS_XAML = os.path.join(VIEWS, "TimesheetPage.xaml")
t = open(TS_XAML, encoding="utf-8").read()

# Fix 1: remove sys:DateTime binding - replace with static text
t = t.replace(
    'Text="{Binding Source={x:Static sys:DateTime.Now}, StringFormat=\'{0:dddd, d MMMM}\'}"',
    'x:Name="DateLabel" Text="Today"'
)

# Fix 2: fix Content set more than once in clock card
# The issue is two Labels inside Border without a layout container
t = t.replace(
    """                <Label Text="■" FontSize="16" TextColor="#ffffff"
                       HorizontalOptions="Center" VerticalOptions="Center"
                       IsVisible="{Binding IsClockedIn}"/>
                <Label Text="▶" FontSize="16" TextColor="#ffffff"
                       HorizontalOptions="Center" VerticalOptions="Center"
                       IsVisible="{Binding IsNotClockedIn}"/>""",
    """                <Grid>
                  <Label Text="■" FontSize="16" TextColor="#ffffff"
                         HorizontalOptions="Center" VerticalOptions="Center"
                         IsVisible="{Binding IsClockedIn}"/>
                  <Label Text="▶" FontSize="16" TextColor="#ffffff"
                         HorizontalOptions="Center" VerticalOptions="Center"
                         IsVisible="{Binding IsNotClockedIn}"/>
                </Grid>"""
)

write(TS_XAML, t)
print("Done! Build and deploy.")