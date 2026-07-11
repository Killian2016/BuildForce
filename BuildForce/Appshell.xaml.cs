namespace BuildForce;
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("TimesheetDetailPage", typeof(Views.TimesheetDetailPage));
        Routing.RegisterRoute("SafetyCheckPage", typeof(Views.SafetyCheckPage));
        Routing.RegisterRoute("InjuryReportPage", typeof(Views.InjuryReportPage));
        Routing.RegisterRoute("ProjectCreatePage", typeof(Views.ProjectCreatePage));
        Routing.RegisterRoute("ExpenseCreatePage", typeof(Views.ExpenseCreatePage));
        
    }
}

