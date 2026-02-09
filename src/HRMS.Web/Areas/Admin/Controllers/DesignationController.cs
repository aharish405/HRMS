using HRMS.Application.DTOs.Designation;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,HRAdmin")]
public class DesignationController : Controller
{
    private readonly IMasterDataService _masterDataService;
    private readonly ILogger<DesignationController> _logger;

    public DesignationController(
        IMasterDataService masterDataService,
        ILogger<DesignationController> logger)
    {
        _masterDataService = masterDataService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _masterDataService.GetAllDesignationsAsync();

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<DesignationDto>());
        }

        return View(result.Data);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDesignationDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _masterDataService.CreateDesignationAsync(model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error creating designation");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _masterDataService.GetDesignationByIdAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var updateDto = new CreateDesignationDto
        {
            Title = result.Data!.Title,
            Description = result.Data.Description,
            Code = result.Data.Code,
            Level = result.Data.Level,
            IsActive = result.Data.IsActive
        };

        ViewBag.DesignationId = id;
        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateDesignationDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.DesignationId = id;
            return View(model);
        }

        var result = await _masterDataService.UpdateDesignationAsync(id, model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error updating designation");
            ViewBag.DesignationId = id;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _masterDataService.DeleteDesignationAsync(id);

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
