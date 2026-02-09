using HRMS.Application.DTOs.Leave;
using HRMS.Shared.Models;

namespace HRMS.Application.Interfaces;

public interface ILeaveService
{
    // Leave Requests
    Task<Result<IEnumerable<LeaveRequestDto>>> GetAllLeaveRequestsAsync();
    Task<Result<LeaveRequestDto>> GetLeaveRequestByIdAsync(int id);
    Task<Result<IEnumerable<LeaveRequestDto>>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
    Task<Result<IEnumerable<LeaveRequestDto>>> GetPendingLeaveRequestsAsync();
    Task<Result<LeaveRequestDto>> CreateLeaveRequestAsync(int employeeId, CreateLeaveRequestDto dto, string createdBy);
    Task<Result<LeaveRequestDto>> ApproveLeaveRequestAsync(int id, ApproveLeaveRequestDto dto, string approvedBy);
    Task<Result> CancelLeaveRequestAsync(int id, string cancelledBy);

    // Leave Balances
    Task<Result<IEnumerable<LeaveBalanceDto>>> GetLeaveBalancesByEmployeeIdAsync(int employeeId, int year);
    Task<Result> InitializeLeaveBalancesAsync(int employeeId, int year);
}
