using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Leave;

public class ApproveLeaveRequestDto
{
    [Required(ErrorMessage = "Approval status is required")]
    public bool IsApproved { get; set; }

    [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
    public string? Comments { get; set; }
}
