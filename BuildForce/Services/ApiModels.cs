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
public class ProjectDetail
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; } = "";
    public decimal Budget { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int InvoiceCount { get; set; }
    public int ExpenseCount { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalExpenses { get; set; }
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
    public DateTime? BreakStartTime { get; set; }
    public int BreakMinutes { get; set; }
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
public class BreakResult
{
    public int TimesheetId { get; set; }
    public DateTime? BreakStartTime { get; set; }
    public int BreakMinutes { get; set; }
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
public class MobileLineItem
{
    public string Description { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public string? Unit { get; set; } = "each";
    public decimal UnitPrice { get; set; }
    public bool IsTaxable { get; set; } = true;
}
public class InvoiceCreateResult
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int LineItemCount { get; set; }
    public string Message { get; set; } = "";
}
public class EstimateCreateResult
{
    public int Id { get; set; }
    public string EstimateNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int LineItemCount { get; set; }
    public string Message { get; set; } = "";
}
public class ReceiptScanPreview
{
    public string? MerchantName { get; set; }
    public string? MerchantAddress { get; set; }
    public string? MerchantPhone { get; set; }
    public DateTime? TransactionDate { get; set; }
    public decimal? Total { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? Tax { get; set; }
    public string? SuggestedCategory { get; set; }
    public string? SuggestedDescription { get; set; }
    public double? Confidence { get; set; }
    public List<ScanItemPreview> Items { get; set; } = new();
}
public class ScanItemPreview
{
    public string? Description { get; set; }
}
// ============================================
// Document browsing: detail models
// Match MobileApiControllers.cs GET /{id} responses
// ============================================
public class LineItemDetail
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}
public class InvoiceDetail
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string Status { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string? Notes { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public List<LineItemDetail>? LineItems { get; set; }
}
public class EstimateSummary
{
    public int Id { get; set; }
    public string EstimateNumber { get; set; } = "";
    public DateTime EstimateDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string Status { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? CustomerName { get; set; }
}
public class EstimateDetail
{
    public int Id { get; set; }
    public string EstimateNumber { get; set; } = "";
    public DateTime EstimateDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string Status { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? CustomerName { get; set; }
    public List<LineItemDetail>? LineItems { get; set; }
}
public class ExpenseSummary
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Category { get; set; }
    public string? Vendor { get; set; }
    public int? ProjectId { get; set; }
    public bool HasReceipt { get; set; }
    public string? ProjectName { get; set; }
}
public class ExpenseDetail
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Category { get; set; }
    public string? Vendor { get; set; }
    public int? ProjectId { get; set; }
    public bool HasReceipt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ProjectName { get; set; }
}
