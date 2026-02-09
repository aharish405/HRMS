using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class Payroll : AuditableEntity
{
    public int EmployeeId { get; set; }

    public int Month { get; set; }
    public int Year { get; set; }

    // Attendance
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

    // Processing Info
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? ProcessedBy { get; set; }

    // Navigation Properties
    public Employee Employee { get; set; } = null!;
}
