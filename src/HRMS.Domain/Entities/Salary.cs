using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class Salary : AuditableEntity
{
    public int EmployeeId { get; set; }

    // Salary Components
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

    // Computed Fields
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public decimal CTC { get; set; }

    // Validity
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public Employee Employee { get; set; } = null!;
}
