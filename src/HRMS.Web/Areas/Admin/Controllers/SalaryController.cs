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

    public async Task<IActionResult> Create(int? employeeId)
    {
        await PopulateEmployeesDropdownAsync(employeeId);

        var model = new CreateSalaryDto
        {
            EffectiveFrom = DateTime.Today
        };

        if (employeeId.HasValue)
        {
            model.EmployeeId = employeeId.Value;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSalaryDto model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }

        var result = await _salaryService.CreateSalaryAsync(model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error creating salary");
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _salaryService.GetSalaryByIdAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var updateDto = new CreateSalaryDto
        {
            EmployeeId = result.Data!.EmployeeId,
            BasicSalary = result.Data.BasicSalary,
            HRA = result.Data.HRA,
            ConveyanceAllowance = result.Data.ConveyanceAllowance,
            MedicalAllowance = result.Data.MedicalAllowance,
            SpecialAllowance = result.Data.SpecialAllowance,
            OtherAllowances = result.Data.OtherAllowances,
            PF = result.Data.PF,
            ESI = result.Data.ESI,
            ProfessionalTax = result.Data.ProfessionalTax,
            TDS = result.Data.TDS,
            OtherDeductions = result.Data.OtherDeductions,
            EffectiveFrom = result.Data.EffectiveFrom,
            EffectiveTo = result.Data.EffectiveTo
        };

        await PopulateEmployeesDropdownAsync(updateDto.EmployeeId);
        ViewBag.SalaryId = id;
        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateSalaryDto model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            ViewBag.SalaryId = id;
            return View(model);
        }

        var result = await _salaryService.UpdateSalaryAsync(id, model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error updating salary");
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            ViewBag.SalaryId = id;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _salaryService.DeleteSalaryAsync(id);

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
