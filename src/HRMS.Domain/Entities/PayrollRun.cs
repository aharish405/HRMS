using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class PayrollRun : AuditableEntity
{
    public int Month { get; set; }
    public int Year { get; set; }
    public PayrollStatus Status { get; set; }

    public DateTime? ProcessedOn { get; set; }
    public string? ProcessedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }
    public string? ApprovedBy { get; set; }

    public DateTime? LockedOn { get; set; }
    public string? LockedBy { get; set; }

    public int TotalEmployees { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }

    // Navigation Properties
    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}
