using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces;
using HRMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.Areas.Employee.Controllers;

[Area("Employee")]
[Authorize(Roles = "Employee")]
public class MyLeavesController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveService _leaveService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MyLeavesController> _logger;

    public MyLeavesController(
        IEmployeeService employeeService,
        ILeaveService leaveService,
        IUnitOfWork unitOfWork,
        ILogger<MyLeavesController> logger)
    {
        _employeeService = employeeService;
        _leaveService = leaveService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var employeeResult = await _employeeService.GetEmployeeByEmailAsync(User.Identity!.Name!);

        if (!employeeResult.Success)
        {
            TempData["ErrorMessage"] = "Unable to load employee information";
            return View();
        }

        var employeeId = employeeResult.Data!.Id;
        var currentYear = DateTime.Now.Year;

        // Get leave balances
        var balancesResult = await _leaveService.GetLeaveBalancesByEmployeeIdAsync(employeeId, currentYear);
        ViewBag.LeaveBalances = balancesResult.Data ?? new List<LeaveBalanceDto>();

        // Get leave requests
        var requestsResult = await _leaveService.GetLeaveRequestsByEmployeeIdAsync(employeeId);
        var leaveRequests = requestsResult.Data ?? new List<LeaveRequestDto>();

        await PopulateLeaveTypesDropdownAsync();

        return View(leaveRequests);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(CreateLeaveRequestDto model)
    {
        var employeeResult = await _employeeService.GetEmployeeByEmailAsync(User.Identity!.Name!);

        if (!employeeResult.Success)
        {
            TempData["ErrorMessage"] = "Unable to process request";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid leave request";
            return RedirectToAction(nameof(Index));
        }

        var result = await _leaveService.CreateLeaveRequestAsync(
            employeeResult.Data!.Id,
            model,
            User.Identity!.Name!);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var employeeResult = await _employeeService.GetEmployeeByEmailAsync(User.Identity!.Name!);

        if (!employeeResult.Success)
        {
            TempData["ErrorMessage"] = "Access denied";
            return RedirectToAction(nameof(Index));
        }

        // Verify the leave request belongs to the logged-in employee
        var leaveRequestResult = await _leaveService.GetLeaveRequestByIdAsync(id);
        if (!leaveRequestResult.Success || leaveRequestResult.Data!.EmployeeId != employeeResult.Data!.Id)
        {
            TempData["ErrorMessage"] = "Access denied";
            return RedirectToAction(nameof(Index));
        }

        var result = await _leaveService.CancelLeaveRequestAsync(id, User.Identity!.Name!);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLeaveTypesDropdownAsync()
    {
        var leaveTypeRepo = _unitOfWork.Repository<Domain.Entities.LeaveType>();
        var leaveTypes = await leaveTypeRepo.FindAsync(lt => lt.IsActive);

        ViewBag.LeaveTypes = new SelectList(leaveTypes, "Id", "Name");
    }
}
