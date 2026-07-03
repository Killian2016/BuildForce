import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

MORE_CS = os.path.join(VIEWS, "MorePage.xaml.cs")
MORE_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.ViewModels;

namespace BuildForce.Views;

public partial class MorePage : ContentPage
{
    readonly MoreViewModel _vm;

    public MorePage(MoreViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    async void OnBackTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//DashboardPage");

    async void OnGroupClockInTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("GroupClockInPage");

    async void OnEmployeesTapped(object sender, EventArgs e)
        => await DisplayAlert("Employees", "Coming soon!", "OK");

    async void OnMapTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("JobMapPage");

    async void OnContractsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("ContractsPage");

    async void OnReportsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("ProfitPerJobPage");

    async void OnEstimatesTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("EstimatesPage");

    async void OnNotificationsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("NotificationsPage");

    async void OnSubscriptionTapped(object sender, EventArgs e)
        => await DisplayAlert("Subscription", "You are on the Professional Plan.", "OK");

    async void OnSettingsTapped(object sender, EventArgs e)
        => await DisplayAlert("Settings", "Coming soon!", "OK");

    async void OnHelpTapped(object sender, EventArgs e)
        => await DisplayAlert("Support", "Email: support@mezanoconstructionmanagementplatform.com", "OK");

    async void OnProjectsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//ProjectsPage");

    async void OnInvoicesTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//InvoicesPage");

    async void OnSignOutTapped(object sender, EventArgs e)
    {
        var ok = await DisplayAlert("Sign Out", "Are you sure?", "Sign Out", "Cancel");
        if (!ok) return;
        _vm.LogoutCommand.Execute(null);
    }
}
"""

# Also fix MorePage.xaml - add Shell.TabBarIsVisible=False
MORE_XAML = os.path.join(VIEWS, "MorePage.xaml")
t = open(MORE_XAML, encoding="utf-8").read()
t = t.replace(
    'Shell.TabBarIsVisible="True"',
    'Shell.TabBarIsVisible="False"'
)
t = t.replace(
    'Shell.NavBarIsVisible="False" Shell.TabBarIsVisible="True"',
    'Shell.NavBarIsVisible="False" Shell.TabBarIsVisible="False"'
)

write(MORE_XAML, t)
write(MORE_CS, MORE_CS_TEXT)
print("Done! Build and deploy.")