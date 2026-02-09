using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Areas.Employee.Controllers;

[Area("Employee")]
[Authorize(Roles = "Employee")]
public class MyPayslipsController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IPayrollService _payrollService;
    private readonly ILogger<MyPayslipsController> _logger;

    public MyPayslipsController(
        IEmployeeService employeeService,
        IPayrollService payrollService,
        ILogger<MyPayslipsController> logger)
    {
        _employeeService = employeeService;
        _payrollService = payrollService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? month, int? year)
    {
        var employeeResult = await _employeeService.GetEmployeeByEmailAsync(User.Identity!.Name!);

        if (!employeeResult.Success)
        {
            TempData["ErrorMessage"] = "Unable to load employee information";
            return View();
        }

        var employeeId = employeeResult.Data!.Id;
        var payrollsResult = await _payrollService.GetPayrollsByEmployeeIdAsync(employeeId);

        var payslips = payrollsResult.Data ?? new List<HRMS.Application.DTOs.Payroll.PayrollDto>();

        // Filter by month/year if provided
        if (month.HasValue && year.HasValue)
        {
            payslips = payslips.Where(p => p.Month == month.Value && p.Year == year.Value).ToList();
        }

        ViewBag.SelectedMonth = month;
        ViewBag.SelectedYear = year;

        return View(payslips);
    }

    public async Task<IActionResult> Details(int id)
    {
        var employeeResult = await _employeeService.GetEmployeeByEmailAsync(User.Identity!.Name!);

        if (!employeeResult.Success)
        {
            TempData["ErrorMessage"] = "Access denied";
            return RedirectToAction(nameof(Index));
        }

        var payrollResult = await _payrollService.GetPayrollByIdAsync(id);

        if (!payrollResult.Success)
        {
            TempData["ErrorMessage"] = payrollResult.Message;
            return RedirectToAction(nameof(Index));
        }

        // Verify the payslip belongs to the logged-in employee
        if (payrollResult.Data!.EmployeeId != employeeResult.Data!.Id)
        {
            TempData["ErrorMessage"] = "Access denied";
            return RedirectToAction(nameof(Index));
        }

        return View(payrollResult.Data);
    }

    public async Task<IActionResult> DownloadPdf(int id)
    {
        var employeeResult = await _employeeService.GetEmployeeByEmailAsync(User.Identity!.Name!);

        if (!employeeResult.Success)
        {
            TempData["ErrorMessage"] = "Access denied";
            return RedirectToAction(nameof(Index));
        }

        var payrollResult = await _payrollService.GetPayrollByIdAsync(id);

        if (!payrollResult.Success || payrollResult.Data!.EmployeeId != employeeResult.Data!.Id)
        {
            TempData["ErrorMessage"] = "Access denied";
            return RedirectToAction(nameof(Index));
        }

        var pdfResult = await _payrollService.GeneratePayslipPdfAsync(id);

        if (!pdfResult.Success)
        {
            TempData["ErrorMessage"] = pdfResult.Message;
            return RedirectToAction(nameof(Index));
        }

        var fileName = $"Payslip_{payrollResult.Data.Month}_{payrollResult.Data.Year}_{payrollResult.Data.EmployeeCode}.pdf";
        return File(pdfResult.Data!, "application/pdf", fileName);
    }
}
