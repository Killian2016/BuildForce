namespace BuildForce.Services;
public class DashboardData
{
    public decimal TotalRevenue { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalProjects { get; set; }
    public int TotalCustomers { get; set; }
    public int PendingInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public decimal Expenses { get; set; }
    public List<ProjectSummary> RecentProjects { get; set; } = new();
    public decimal Revenue => TotalRevenue;
    public decimal Pending => OutstandingBalance;
    public decimal NetProfit => TotalRevenue - Expenses;
}
public class ProjectSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? CustomerName { get; set; }
    public string Client => CustomerName ?? "";
    public string Status { get; set; } = "";
    public string Location { get; set; } = "";
}
public class InvoiceSummary
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public string Number => InvoiceNumber;
    public string? ProjectName { get; set; }
    public string Project => ProjectName ?? "";
    public string? CustomerName { get; set; }
    public string Client => CustomerName ?? "";
    public decimal TotalAmount { get; set; }
    public decimal Amount => TotalAmount;
    public string Status { get; set; } = "";
    public DateTime InvoiceDate { get; set; }
    public DateTime Date => InvoiceDate;
}
public class LoginResult
{
    public bool Success { get; set; }
    public string Token { get; set; } = "";
    public string Message { get; set; } = "";
}
public class TimesheetEntry
{
    public int TimesheetId { get; set; }
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }
    public DateTime Date { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal TotalHours => HoursWorked + OvertimeHours;
    public string? Description { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public double? ClockInLatitude { get; set; }
    public double? ClockInLongitude { get; set; }
    public double? ClockOutLatitude { get; set; }
    public double? ClockOutLongitude { get; set; }
    public string? EmployeeName { get; set; }
    public string? ProjectName { get; set; }
}
public class ClockInResult
{
    public int TimesheetId { get; set; }
    public int ProjectId { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "";
    public DateTime? ClockInTime { get; set; }
    public string Message { get; set; } = "";
}
public class ClockOutResult
{
    public int TimesheetId { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal TotalHours { get; set; }
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
}
public class TimesheetSummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRegularHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalHours { get; set; }
    public int DaysWorked { get; set; }
    public decimal AverageHoursPerDay { get; set; }
}
public class CustomerSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
public class ProjectCreateResult
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
}
public class ExpenseCreateResult
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string Message { get; set; } = "";
}
