import os

ROOT = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

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

# Check what pages exist first
VIEWS = os.path.join(ROOT, "Views")
files = os.listdir(VIEWS)
page_files = [f for f in files if f.endswith('.xaml.cs') and 'Page' in f]
print("Pages found:")
for f in sorted(page_files):
    print(" ", f)

# Write simple AppShell without the missing pages
SHELL_CS_SIMPLE = """using BuildForce.Views;

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
    }
}
"""
write(SHELL_CS, SHELL_CS_SIMPLE)
print("Done!")