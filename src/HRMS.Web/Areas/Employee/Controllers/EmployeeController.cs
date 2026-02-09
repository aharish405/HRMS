using HRMS.Application.DTOs.Employee;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces;
using HRMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Areas.Employee.Controllers;

[Area("Employee")]
[Authorize(Roles = "Employee")]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveService _leaveService;
    private readonly IPayrollService _payrollService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(
        IEmployeeService employeeService,
        ILeaveService leaveService,
        IPayrollService payrollService,
        ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _leaveService = leaveService;
        _payrollService = payrollService;
        _logger = logger;
    }

    public async Task<IActionResult> Dashboard()
    {
        var employeeResult = await _employeeService.GetEmployeeByEmailAsync(User.Identity!.Name!);

        if (!employeeResult.Success)
        {
            TempData["ErrorMessage"] = "Unable to load employee information";
            return View();
        }

        var employee = employeeResult.Data!;
        var currentYear = DateTime.Now.Year;

        // Get leave balances
        var leaveBalancesResult = await _leaveService.GetLeaveBalancesByEmployeeIdAsync(employee.Id, currentYear);
        ViewBag.LeaveBalances = leaveBalancesResult.Data ?? new List<LeaveBalanceDto>();

        // Get recent payslips (last 3 months)
        var payrollsResult = await _payrollService.GetPayrollsByEmployeeIdAsync(employee.Id);
        ViewBag.RecentPayslips = payrollsResult.Data?.Take(3) ?? new List<PayrollDto>();

        // Get pending leave requests
        var leaveRequestsResult = await _leaveService.GetLeaveRequestsByEmployeeIdAsync(employee.Id);
        ViewBag.PendingLeaves = leaveRequestsResult.Data?
            .Where(l => l.Status == "Pending")
            .Take(5) ?? new List<LeaveRequestDto>();

        return View(employee);
    }

    public async Task<IActionResult> Profile()
    {
        var employeeResult = await _employeeService.GetEmployeeByEmailAsync(User.Identity!.Name!);

        if (!employeeResult.Success)
        {
            TempData["ErrorMessage"] = "Unable to load profile";
            return RedirectToAction(nameof(Dashboard));
        }

        return View(employeeResult.Data);
    }
}
