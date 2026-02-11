using HRMS.Application.DTOs.Salary;
using HRMS.Application.Interfaces;
using HRMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,HRAdmin")]
public class SalaryController : Controller
{
    private readonly ISalaryService _salaryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SalaryController> _logger;

    public SalaryController(
        ISalaryService salaryService,
        IUnitOfWork unitOfWork,
        ILogger<SalaryController> logger)
    {
        _salaryService = salaryService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _salaryService.GetAllSalariesAsync();

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<SalaryDto>());
        }

        return View(result.Data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _salaryService.GetSalaryByIdAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    public async Task<IActionResult> History(int employeeId)
    {
        var result = await _salaryService.GetSalaryHistoryByEmployeeIdAsync(employeeId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Index", "Employee");
        }

        ViewBag.EmployeeId = employeeId;
        return View(result.Data);
    }
}
