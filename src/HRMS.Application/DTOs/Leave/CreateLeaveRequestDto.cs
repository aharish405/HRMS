using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Leave;

public class CreateLeaveRequestDto
{
    [Required(ErrorMessage = "Leave type is required")]
    public int LeaveTypeId { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Reason is required")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
}
