namespace BuildForce;
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("TimesheetDetailPage", typeof(Views.TimesheetDetailPage));
    }
}