using AutoMapper;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Models;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services;

public class LeaveService : ILeaveService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<LeaveService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<LeaveRequestDto>>> GetAllLeaveRequestsAsync()
    {
        try
        {
            var leaveRequestRepo = _unitOfWork.Repository<LeaveRequest>();
            var leaveRequests = await leaveRequestRepo.GetAllAsync();

            var leaveRequestsList = leaveRequests.OrderByDescending(lr => lr.CreatedOn).ToList();
            foreach (var leaveRequest in leaveRequestsList)
            {
                await LoadNavigationPropertiesAsync(leaveRequest);
            }

            var leaveRequestDtos = _mapper.Map<IEnumerable<LeaveRequestDto>>(leaveRequestsList);
            return Result<IEnumerable<LeaveRequestDto>>.SuccessResult(leaveRequestDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all leave requests");
            return Result<IEnumerable<LeaveRequestDto>>.FailureResult("Error retrieving leave requests");
        }
    }

    public async Task<Result<LeaveRequestDto>> GetLeaveRequestByIdAsync(int id)
    {
        try
        {
            var leaveRequestRepo = _unitOfWork.Repository<LeaveRequest>();
            var leaveRequest = await leaveRequestRepo.GetByIdAsync(id);

            if (leaveRequest == null)
            {
                return Result<LeaveRequestDto>.FailureResult("Leave request not found");
            }

            await LoadNavigationPropertiesAsync(leaveRequest);

            var leaveRequestDto = _mapper.Map<LeaveRequestDto>(leaveRequest);
            return Result<LeaveRequestDto>.SuccessResult(leaveRequestDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave request {LeaveRequestId}", id);
            return Result<LeaveRequestDto>.FailureResult("Error retrieving leave request");
        }
    }

    public async Task<Result<IEnumerable<LeaveRequestDto>>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
    {
        try
        {
            var leaveRequestRepo = _unitOfWork.Repository<LeaveRequest>();
            var leaveRequests = await leaveRequestRepo.FindAsync(lr => lr.EmployeeId == employeeId);

            var leaveRequestsList = leaveRequests.OrderByDescending(lr => lr.CreatedOn).ToList();
            foreach (var leaveRequest in leaveRequestsList)
            {
                await LoadNavigationPropertiesAsync(leaveRequest);
            }

            var leaveRequestDtos = _mapper.Map<IEnumerable<LeaveRequestDto>>(leaveRequestsList);
            return Result<IEnumerable<LeaveRequestDto>>.SuccessResult(leaveRequestDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave requests for employee {EmployeeId}", employeeId);
            return Result<IEnumerable<LeaveRequestDto>>.FailureResult("Error retrieving leave requests");
        }
    }

    public async Task<Result<IEnumerable<LeaveRequestDto>>> GetPendingLeaveRequestsAsync()
    {
        try
        {
            var leaveRequestRepo = _unitOfWork.Repository<LeaveRequest>();
            var leaveRequests = await leaveRequestRepo.FindAsync(lr => lr.Status == LeaveStatus.Pending);

            var leaveRequestsList = leaveRequests.OrderBy(lr => lr.StartDate).ToList();
            foreach (var leaveRequest in leaveRequestsList)
            {
                await LoadNavigationPropertiesAsync(leaveRequest);
            }

            var leaveRequestDtos = _mapper.Map<IEnumerable<LeaveRequestDto>>(leaveRequestsList);
            return Result<IEnumerable<LeaveRequestDto>>.SuccessResult(leaveRequestDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending leave requests");
            return Result<IEnumerable<LeaveRequestDto>>.FailureResult("Error retrieving pending leave requests");
        }
    }

    public async Task<Result<LeaveRequestDto>> CreateLeaveRequestAsync(int employeeId, CreateLeaveRequestDto dto, string createdBy)
    {
        try
        {
            // Validate dates
            if (dto.EndDate < dto.StartDate)
            {
                return Result<LeaveRequestDto>.FailureResult("End date cannot be before start date");
            }

            // Calculate number of days
            var numberOfDays = (dto.EndDate - dto.StartDate).Days + 1;

            // Check leave balance
            var leaveBalanceRepo = _unitOfWork.Repository<LeaveBalance>();
            var currentYear = DateTime.Now.Year;
            var leaveBalances = await leaveBalanceRepo.FindAsync(lb =>
                lb.EmployeeId == employeeId &&
                lb.LeaveTypeId == dto.LeaveTypeId &&
                lb.Year == currentYear);

            var leaveBalance = leaveBalances.FirstOrDefault();
            if (leaveBalance == null)
            {
                // Initialize leave balance if not exists
                await InitializeLeaveBalancesAsync(employeeId, currentYear);
                leaveBalances = await leaveBalanceRepo.FindAsync(lb =>
                    lb.EmployeeId == employeeId &&
                    lb.LeaveTypeId == dto.LeaveTypeId &&
                    lb.Year == currentYear);
                leaveBalance = leaveBalances.FirstOrDefault();
            }

            if (leaveBalance != null && leaveBalance.AvailableDays < numberOfDays)
            {
                return Result<LeaveRequestDto>.FailureResult(
                    $"Insufficient leave balance. Available: {leaveBalance.AvailableDays} days, Requested: {numberOfDays} days");
            }

            // Create leave request
            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employeeId,
                LeaveTypeId = dto.LeaveTypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                NumberOfDays = numberOfDays,
                Reason = dto.Reason,
                Status = LeaveStatus.Pending,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow
            };

            var leaveRequestRepo = _unitOfWork.Repository<LeaveRequest>();
            await leaveRequestRepo.AddAsync(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Leave request created for employee {EmployeeId} by {CreatedBy}", employeeId, createdBy);

            await LoadNavigationPropertiesAsync(leaveRequest);
            var leaveRequestDto = _mapper.Map<LeaveRequestDto>(leaveRequest);

            return Result<LeaveRequestDto>.SuccessResult(leaveRequestDto, "Leave request submitted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave request");
            return Result<LeaveRequestDto>.FailureResult("Error creating leave request");
        }
    }

    public async Task<Result<LeaveRequestDto>> ApproveLeaveRequestAsync(int id, ApproveLeaveRequestDto dto, string approvedBy)
    {
        try
        {
            var leaveRequestRepo = _unitOfWork.Repository<LeaveRequest>();
            var leaveRequest = await leaveRequestRepo.GetByIdAsync(id);

            if (leaveRequest == null)
            {
                return Result<LeaveRequestDto>.FailureResult("Leave request not found");
            }

            if (leaveRequest.Status != LeaveStatus.Pending)
            {
                return Result<LeaveRequestDto>.FailureResult("Only pending leave requests can be approved/rejected");
            }

            // Update leave request status
            leaveRequest.Status = dto.IsApproved ? LeaveStatus.Approved : LeaveStatus.Rejected;
            leaveRequest.ApprovedBy = approvedBy;
            leaveRequest.ApprovedOn = DateTime.UtcNow;
            leaveRequest.ApprovalComments = dto.Comments;
            leaveRequest.ModifiedBy = approvedBy;
            leaveRequest.ModifiedOn = DateTime.UtcNow;

            // If approved, update leave balance
            if (dto.IsApproved)
            {
                var leaveBalanceRepo = _unitOfWork.Repository<LeaveBalance>();
                var currentYear = DateTime.Now.Year;
                var leaveBalances = await leaveBalanceRepo.FindAsync(lb =>
                    lb.EmployeeId == leaveRequest.EmployeeId &&
                    lb.LeaveTypeId == leaveRequest.LeaveTypeId &&
                    lb.Year == currentYear);

                var leaveBalance = leaveBalances.FirstOrDefault();
                if (leaveBalance != null)
                {
                    leaveBalance.UsedDays += (int)leaveRequest.NumberOfDays;
                    leaveBalance.AvailableDays -= (int)leaveRequest.NumberOfDays;
                    leaveBalanceRepo.Update(leaveBalance);
                }
            }

            leaveRequestRepo.Update(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Leave request {LeaveRequestId} {Status} by {ApprovedBy}",
                id, leaveRequest.Status, approvedBy);

            await LoadNavigationPropertiesAsync(leaveRequest);
            var leaveRequestDto = _mapper.Map<LeaveRequestDto>(leaveRequest);

            var message = dto.IsApproved ? "Leave request approved successfully" : "Leave request rejected";
            return Result<LeaveRequestDto>.SuccessResult(leaveRequestDto, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving leave request {LeaveRequestId}", id);
            return Result<LeaveRequestDto>.FailureResult("Error processing leave request");
        }
    }

    public async Task<Result> CancelLeaveRequestAsync(int id, string cancelledBy)
    {
        try
        {
            var leaveRequestRepo = _unitOfWork.Repository<LeaveRequest>();
            var leaveRequest = await leaveRequestRepo.GetByIdAsync(id);

            if (leaveRequest == null)
            {
                return Result.FailureResult("Leave request not found");
            }

            if (leaveRequest.Status != LeaveStatus.Pending)
            {
                return Result.FailureResult("Only pending leave requests can be cancelled");
            }

            leaveRequest.Status = LeaveStatus.Cancelled;
            leaveRequest.ModifiedBy = cancelledBy;
            leaveRequest.ModifiedOn = DateTime.UtcNow;

            leaveRequestRepo.Update(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Leave request {LeaveRequestId} cancelled by {CancelledBy}", id, cancelledBy);

            return Result.SuccessResult("Leave request cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling leave request {LeaveRequestId}", id);
            return Result.FailureResult("Error cancelling leave request");
        }
    }

    public async Task<Result<IEnumerable<LeaveBalanceDto>>> GetLeaveBalancesByEmployeeIdAsync(int employeeId, int year)
    {
        try
        {
            var leaveBalanceRepo = _unitOfWork.Repository<LeaveBalance>();
            var leaveBalances = await leaveBalanceRepo.FindAsync(lb =>
                lb.EmployeeId == employeeId && lb.Year == year);

            var leaveBalancesList = leaveBalances.ToList();
            foreach (var leaveBalance in leaveBalancesList)
            {
                await LoadLeaveBalanceNavigationPropertiesAsync(leaveBalance);
            }

            var leaveBalanceDtos = _mapper.Map<IEnumerable<LeaveBalanceDto>>(leaveBalancesList);
            return Result<IEnumerable<LeaveBalanceDto>>.SuccessResult(leaveBalanceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave balances for employee {EmployeeId}", employeeId);
            return Result<IEnumerable<LeaveBalanceDto>>.FailureResult("Error retrieving leave balances");
        }
    }

    public async Task<Result> InitializeLeaveBalancesAsync(int employeeId, int year)
    {
        try
        {
            var leaveTypeRepo = _unitOfWork.Repository<LeaveType>();
            var leaveBalanceRepo = _unitOfWork.Repository<LeaveBalance>();

            var activeLeaveTypes = await leaveTypeRepo.FindAsync(lt => lt.IsActive);

            foreach (var leaveType in activeLeaveTypes)
            {
                // Check if balance already exists
                var existingBalances = await leaveBalanceRepo.FindAsync(lb =>
                    lb.EmployeeId == employeeId &&
                    lb.LeaveTypeId == leaveType.Id &&
                    lb.Year == year);

                if (!existingBalances.Any())
                {
                    var leaveBalance = new LeaveBalance
                    {
                        EmployeeId = employeeId,
                        LeaveTypeId = leaveType.Id,
                        Year = year,
                        TotalDays = leaveType.DefaultDaysPerYear,
                        UsedDays = 0,
                        AvailableDays = leaveType.DefaultDaysPerYear,
                        CreatedBy = "System",
                        CreatedOn = DateTime.UtcNow
                    };

                    await leaveBalanceRepo.AddAsync(leaveBalance);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Leave balances initialized for employee {EmployeeId} for year {Year}", employeeId, year);

            return Result.SuccessResult("Leave balances initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing leave balances for employee {EmployeeId}", employeeId);
            return Result.FailureResult("Error initializing leave balances");
        }
    }

    private async Task LoadNavigationPropertiesAsync(LeaveRequest leaveRequest)
    {
        var employeeRepo = _unitOfWork.Repository<Employee>();
        var departmentRepo = _unitOfWork.Repository<Department>();
        var leaveTypeRepo = _unitOfWork.Repository<LeaveType>();

        leaveRequest.Employee = (await employeeRepo.GetByIdAsync(leaveRequest.EmployeeId))!;
        leaveRequest.Employee.Department = (await departmentRepo.GetByIdAsync(leaveRequest.Employee.DepartmentId))!;
        leaveRequest.LeaveType = (await leaveTypeRepo.GetByIdAsync(leaveRequest.LeaveTypeId))!;
    }

    private async Task LoadLeaveBalanceNavigationPropertiesAsync(LeaveBalance leaveBalance)
    {
        var employeeRepo = _unitOfWork.Repository<Employee>();
        var leaveTypeRepo = _unitOfWork.Repository<LeaveType>();

        leaveBalance.Employee = (await employeeRepo.GetByIdAsync(leaveBalance.EmployeeId))!;
        leaveBalance.LeaveType = (await leaveTypeRepo.GetByIdAsync(leaveBalance.LeaveTypeId))!;
    }
}
