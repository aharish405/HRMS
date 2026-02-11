using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class EmployeeAddress : AuditableEntity
{
    public int EmployeeId { get; set; }

    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public AddressType Type { get; set; }
    public bool IsPrimary { get; set; }

    // Navigation Properties
    public Employee Employee { get; set; } = null!;
}
