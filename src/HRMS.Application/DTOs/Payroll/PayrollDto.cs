namespace HRMS.Application.DTOs.Payroll;

public class PayrollDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DesignationTitle { get; set; } = string.Empty;

    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthYear => $"{GetMonthName(Month)} {Year}";

    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int LeaveDays { get; set; }
    public int AbsentDays { get; set; }

    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossSalary { get; set; }

    // Deductions
    public decimal PF { get; set; }
    public decimal ESI { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal TDS { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }

    // Net Salary
    public decimal NetSalary { get; set; }

    public bool IsProcessed { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? ProcessedBy { get; set; }

    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    private static string GetMonthName(int month)
    {
        return new DateTime(2000, month, 1).ToString("MMMM");
    }
}
