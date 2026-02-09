using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Payroll;

public class GeneratePayrollDto
{
    [Required(ErrorMessage = "Month is required")]
    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
    public int Month { get; set; }

    [Required(ErrorMessage = "Year is required")]
    [Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100")]
    public int Year { get; set; }

    public int? EmployeeId { get; set; } // If null, generate for all employees
}
