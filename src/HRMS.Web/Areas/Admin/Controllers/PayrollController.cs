using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,HRAdmin")]
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly ILogger<PayrollController> _logger;

    public PayrollController(
        IPayrollService payrollService,
        ILogger<PayrollController> logger)
    {
        _payrollService = payrollService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? month, int? year)
    {
        var currentMonth = month ?? DateTime.Now.Month;
        var currentYear = year ?? DateTime.Now.Year;

        var result = await _payrollService.GetPayrollsByMonthYearAsync(currentMonth, currentYear);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            ViewBag.Payrolls = new List<PayrollDto>();
        }
        else
        {
            ViewBag.Payrolls = result.Data;
        }

        ViewBag.SelectedMonth = currentMonth;
        ViewBag.SelectedYear = currentYear;

        return View();
    }

    public IActionResult Generate()
    {
        var model = new GeneratePayrollDto
        {
            Month = DateTime.Now.Month,
            Year = DateTime.Now.Year
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(GeneratePayrollDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _payrollService.GeneratePayrollAsync(model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error generating payroll");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index), new { month = model.Month, year = model.Year });
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _payrollService.GetPayrollByIdAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    public async Task<IActionResult> DownloadPayslip(int id)
    {
        var result = await _payrollService.GeneratePayslipPdfAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var payrollResult = await _payrollService.GetPayrollByIdAsync(id);
        var fileName = $"Payslip_{payrollResult.Data!.EmployeeCode}_{payrollResult.Data.MonthYear.Replace(" ", "_")}.pdf";

        return File(result.Data!, "application/pdf", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _payrollService.DeletePayrollAsync(id);

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
}
