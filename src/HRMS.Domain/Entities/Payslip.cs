using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class Payslip : AuditableEntity
{
    public int EmployeeId { get; set; }
    public int PayrollRunId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    // Salary Breakdown (snapshot from Salary table)
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal OtherAllowances { get; set; }

    // Deductions
    public decimal PF { get; set; }
    public decimal ESI { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal TDS { get; set; }
    public decimal OtherDeductions { get; set; }

    // Adjustments
    public decimal LossOfPayDays { get; set; }
    public decimal LossOfPayAmount { get; set; }

    // Totals
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }

    // File Storage
    public string? FilePath { get; set; }
    public bool IsLocked { get; set; }

    // Navigation Properties
    public Employee Employee { get; set; } = null!;
    public PayrollRun PayrollRun { get; set; } = null!;
}
