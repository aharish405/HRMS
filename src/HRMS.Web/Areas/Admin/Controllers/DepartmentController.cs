using HRMS.Application.DTOs.Department;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,HRAdmin")]
public class DepartmentController : Controller
{
    private readonly IMasterDataService _masterDataService;
    private readonly ILogger<DepartmentController> _logger;

    public DepartmentController(
        IMasterDataService masterDataService,
        ILogger<DepartmentController> logger)
    {
        _masterDataService = masterDataService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _masterDataService.GetAllDepartmentsAsync();

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<DepartmentDto>());
        }

        return View(result.Data);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDepartmentDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _masterDataService.CreateDepartmentAsync(model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error creating department");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _masterDataService.GetDepartmentByIdAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var updateDto = new CreateDepartmentDto
        {
            Name = result.Data!.Name,
            Description = result.Data.Description,
            Code = result.Data.Code,
            IsActive = result.Data.IsActive
        };

        ViewBag.DepartmentId = id;
        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateDepartmentDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.DepartmentId = id;
            return View(model);
        }

        var result = await _masterDataService.UpdateDepartmentAsync(id, model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error updating department");
            ViewBag.DepartmentId = id;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _masterDataService.DeleteDepartmentAsync(id);

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
