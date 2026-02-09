using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces;
using HRMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
public class LeaveController : Controller
{
    private readonly ILeaveService _leaveService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LeaveController> _logger;

    public LeaveController(
        ILeaveService leaveService,
        IUnitOfWork unitOfWork,
        ILogger<LeaveController> logger)
    {
        _leaveService = leaveService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _leaveService.GetAllLeaveRequestsAsync();

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<LeaveRequestDto>());
        }

        return View(result.Data);
    }

    public async Task<IActionResult> Pending()
    {
        var result = await _leaveService.GetPendingLeaveRequestsAsync();

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<LeaveRequestDto>());
        }

        return View(result.Data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _leaveService.GetLeaveRequestByIdAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    public async Task<IActionResult> Create(int? employeeId)
    {
        await PopulateLeaveTypesDropdownAsync();

        if (employeeId.HasValue)
        {
            await PopulateEmployeesDropdownAsync(employeeId);
            ViewBag.SelectedEmployeeId = employeeId.Value;
        }
        else
        {
            await PopulateEmployeesDropdownAsync(null);
        }

        var model = new CreateLeaveRequestDto
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int employeeId, CreateLeaveRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLeaveTypesDropdownAsync();
            await PopulateEmployeesDropdownAsync(employeeId);
            ViewBag.SelectedEmployeeId = employeeId;
            return View(model);
        }

        var result = await _leaveService.CreateLeaveRequestAsync(employeeId, model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error creating leave request");
            await PopulateLeaveTypesDropdownAsync();
            await PopulateEmployeesDropdownAsync(employeeId);
            ViewBag.SelectedEmployeeId = employeeId;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, bool isApproved, string? comments)
    {
        var dto = new ApproveLeaveRequestDto
        {
            IsApproved = isApproved,
            Comments = comments
        };

        var result = await _leaveService.ApproveLeaveRequestAsync(id, dto, User.Identity!.Name!);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Pending));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
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

    public async Task<IActionResult> Balance(int employeeId)
    {
        var currentYear = DateTime.Now.Year;
        var result = await _leaveService.GetLeaveBalancesByEmployeeIdAsync(employeeId, currentYear);

        if (!result.Success || !result.Data!.Any())
        {
            // Initialize balances if not found
            await _leaveService.InitializeLeaveBalancesAsync(employeeId, currentYear);
            result = await _leaveService.GetLeaveBalancesByEmployeeIdAsync(employeeId, currentYear);
        }

        ViewBag.EmployeeId = employeeId;
        ViewBag.Year = currentYear;

        return View(result.Data);
    }

    private async Task PopulateLeaveTypesDropdownAsync()
    {
        var leaveTypeRepo = _unitOfWork.Repository<Domain.Entities.LeaveType>();
        var leaveTypes = await leaveTypeRepo.FindAsync(lt => lt.IsActive);

        ViewBag.LeaveTypes = new SelectList(leaveTypes, "Id", "Name");
    }

    private async Task PopulateEmployeesDropdownAsync(int? selectedEmployee = null)
    {
        var employeeRepo = _unitOfWork.Repository<Domain.Entities.Employee>();
        var employees = await employeeRepo.FindAsync(e => e.Status == Domain.Enums.EmployeeStatus.Active);

        ViewBag.Employees = new SelectList(
            employees.Select(e => new { e.Id, FullName = $"{e.EmployeeCode} - {e.FirstName} {e.LastName}" }),
            "Id",
            "FullName",
            selectedEmployee);
    }
}
