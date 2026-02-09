using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Salary;

public class CreateSalaryDto
{
    [Required(ErrorMessage = "Employee is required")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Basic salary is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Basic salary must be positive")]
    public decimal BasicSalary { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "HRA must be positive")]
    public decimal HRA { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Conveyance allowance must be positive")]
    public decimal ConveyanceAllowance { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Medical allowance must be positive")]
    public decimal MedicalAllowance { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Special allowance must be positive")]
    public decimal SpecialAllowance { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Other allowances must be positive")]
    public decimal OtherAllowances { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "PF must be positive")]
    public decimal PF { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "ESI must be positive")]
    public decimal ESI { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Professional tax must be positive")]
    public decimal ProfessionalTax { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "TDS must be positive")]
    public decimal TDS { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Other deductions must be positive")]
    public decimal OtherDeductions { get; set; }

    [Required(ErrorMessage = "Effective from date is required")]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }
}
