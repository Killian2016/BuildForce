import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

# ── AppShell.xaml — clean version with all routes ──────────────
SHELL = os.path.join(ROOT, "AppShell.xaml")
SHELL_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<Shell
    x:Class="BuildForce.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:views="clr-namespace:BuildForce.Views"
    Shell.NavBarIsVisible="False"
    Shell.TabBarIsVisible="False">
    <TabBar>
        <Tab Title="Dashboard">
            <ShellContent ContentTemplate="{DataTemplate views:DashboardPage}" Route="DashboardPage"/>
        </Tab>
        <Tab Title="Projects">
            <ShellContent ContentTemplate="{DataTemplate views:ProjectsPage}" Route="ProjectsPage"/>
        </Tab>
        <Tab Title="Invoices">
            <ShellContent ContentTemplate="{DataTemplate views:InvoicesPage}" Route="InvoicesPage"/>
        </Tab>
        <Tab Title="Expenses">
            <ShellContent ContentTemplate="{DataTemplate views:ExpensesPage}" Route="ExpensesPage"/>
        </Tab>
        <Tab Title="Time">
            <ShellContent ContentTemplate="{DataTemplate views:TimesheetPage}" Route="TimesheetPage"/>
        </Tab>
        <Tab Title="Customers">
            <ShellContent ContentTemplate="{DataTemplate views:CustomersPage}" Route="CustomersPage"/>
        </Tab>
        <Tab Title="More">
            <ShellContent ContentTemplate="{DataTemplate views:MorePage}" Route="MorePage"/>
        </Tab>
    </TabBar>
</Shell>
"""

write(SHELL, SHELL_TEXT)

# ── AppShell.xaml.cs — register all detail routes ──────────────
SHELL_CS = os.path.join(ROOT, "AppShell.xaml.cs")
SHELL_CS_TEXT = """using BuildForce.Views;

namespace BuildForce;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(TimesheetDetailPage), typeof(TimesheetDetailPage));
        Routing.RegisterRoute(nameof(ProjectDetailPage),   typeof(ProjectDetailPage));
        Routing.RegisterRoute(nameof(InvoiceDetailPage),   typeof(InvoiceDetailPage));
        Routing.RegisterRoute(nameof(CreateInvoicePage),   typeof(CreateInvoicePage));
        Routing.RegisterRoute(nameof(CollectPaymentPage),  typeof(CollectPaymentPage));
        Routing.RegisterRoute(nameof(EstimatesPage),       typeof(EstimatesPage));
        Routing.RegisterRoute(nameof(EstimateDetailPage),  typeof(EstimateDetailPage));
        Routing.RegisterRoute(nameof(NotificationsPage),   typeof(NotificationsPage));
        Routing.RegisterRoute(nameof(GroupClockInPage),    typeof(GroupClockInPage));
        Routing.RegisterRoute("JobMapPage",                typeof(JobMapPage));
        Routing.RegisterRoute("ContractsPage",             typeof(DigitalContractPage));
        Routing.RegisterRoute("ProfitPerJobPage",          typeof(ProfitPerJobPage));
    }
}
"""

write(SHELL_CS, SHELL_CS_TEXT)
print("\\nDone! Build and deploy.")