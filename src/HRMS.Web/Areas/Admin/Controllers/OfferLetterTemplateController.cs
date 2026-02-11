using HRMS.Application.DTOs.OfferLetter;
using HRMS.Application.DTOs.OfferLetterTemplate;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,HRAdmin")]
public class OfferLetterTemplateController : Controller
{
    private readonly IOfferLetterTemplateService _templateService;
    private readonly IOfferLetterService _offerLetterService;
    private readonly IMasterDataService _masterDataService;
    private readonly ILogger<OfferLetterTemplateController> _logger;

    public OfferLetterTemplateController(
        IOfferLetterTemplateService templateService,
        IOfferLetterService offerLetterService,
        IMasterDataService masterDataService,
        ILogger<OfferLetterTemplateController> logger)
    {
        _templateService = templateService;
        _offerLetterService = offerLetterService;
        _masterDataService = masterDataService;
        _logger = logger;
    }

    // GET: Admin/OfferLetterTemplate
    public async Task<IActionResult> Index()
    {
        var result = await _templateService.GetAllTemplatesAsync();
        if (result.Success)
        {
            return View(result.Data);
        }

        TempData["ErrorMessage"] = result.Message;
        return View(new List<OfferLetterTemplateDto>());
    }

    // GET: Admin/OfferLetterTemplate/Create
    public IActionResult Create()
    {
        return View(new CreateOfferLetterTemplateDto());
    }

    // POST: Admin/OfferLetterTemplate/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOfferLetterTemplateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
        var result = await _templateService.CreateTemplateAsync(dto, userName);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        return View(dto);
    }

    // GET: Admin/OfferLetterTemplate/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _templateService.GetTemplateByIdAsync(id);
        if (!result.Success || result.Data == null)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var updateDto = new UpdateOfferLetterTemplateDto
        {
            Id = result.Data!.Id,
            Name = result.Data.Name,
            Description = result.Data.Description,
            Content = result.Data.Content,
            Category = result.Data.Category,
            IsActive = result.Data.IsActive,
            IsDefault = result.Data.IsDefault
        };

        return View(updateDto);
    }

    // POST: Admin/OfferLetterTemplate/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateOfferLetterTemplateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
        var result = await _templateService.UpdateTemplateAsync(dto, userName);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        return View(dto);
    }

    // POST: Admin/OfferLetterTemplate/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _templateService.DeleteTemplateAsync(id);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/OfferLetterTemplate/SetDefault/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefault(int id)
    {
        var result = await _templateService.SetDefaultTemplateAsync(id);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/OfferLetterTemplate/Clone/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(int id)
    {
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
        var result = await _templateService.CloneTemplateAsync(id, userName);

        if (result.Success && result.Data != null)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Edit), new { id = result.Data!.Id });
        }

        TempData["ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/OfferLetterTemplate/Preview/5
    public async Task<IActionResult> Preview(int id)
    {
        var result = await _templateService.GetTemplateByIdAsync(id);
        if (!result.Success || result.Data == null)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // Get placeholders for preview
        var placeholdersResult = await _templateService.GetAllPlaceholdersAsync();
        var placeholderValues = new Dictionary<string, string>();

        if (placeholdersResult.Success && placeholdersResult.Data != null)
        {
            foreach (var placeholder in placeholdersResult.Data!)
            {
                placeholderValues[placeholder.PlaceholderKey] = placeholder.SampleValue ?? "";
            }
        }

        var previewResult = await _templateService.PreviewTemplateAsync(id, placeholderValues);

        ViewBag.TemplateName = result.Data!.Name;
        ViewBag.PreviewContent = previewResult.Success ? previewResult.Data : "Error generating preview";

        return View();
    }

    // GET: Admin/OfferLetterTemplate/Placeholders
    public async Task<IActionResult> Placeholders()
    {
        var result = await _templateService.GetAllPlaceholdersAsync();
        if (result.Success)
        {
            return View(result.Data);
        }

        TempData["ErrorMessage"] = result.Message;
        return View(new List<TemplatePlaceholderDto>());
    }

    // GET: Admin/OfferLetterTemplate/BulkGenerate
    public async Task<IActionResult> BulkGenerate()
    {
        // Get templates for selection
        var templatesResult = await _templateService.GetAllTemplatesAsync();
        var templates = templatesResult.Success ? templatesResult.Data : new List<OfferLetterTemplateDto>();
        ViewBag.Templates = templates;

        // Get departments and designations for dropdowns
        var departmentsResult = await _masterDataService.GetAllDepartmentsAsync();
        var designationsResult = await _masterDataService.GetAllDesignationsAsync();

        ViewBag.Departments = departmentsResult.Success ? departmentsResult.Data : new List<Application.DTOs.Department.DepartmentDto>();
        ViewBag.Designations = designationsResult.Success ? designationsResult.Data : new List<Application.DTOs.Designation.DesignationDto>();

        return View(new BulkOfferGenerationDto());
    }

    // POST: Admin/OfferLetterTemplate/BulkGenerate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkGenerate(BulkOfferGenerationDto dto)
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdowns
            var templatesResult = await _templateService.GetAllTemplatesAsync();
            ViewBag.Templates = templatesResult.Success ? templatesResult.Data : new List<OfferLetterTemplateDto>();

            var departmentsResult = await _masterDataService.GetAllDepartmentsAsync();
            var designationsResult = await _masterDataService.GetAllDesignationsAsync();

            ViewBag.Departments = departmentsResult.Success ? departmentsResult.Data : new List<Application.DTOs.Department.DepartmentDto>();
            ViewBag.Designations = designationsResult.Success ? designationsResult.Data : new List<Application.DTOs.Designation.DesignationDto>();

            return View(dto);
        }

        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
        var result = await _offerLetterService.BulkGenerateOfferLettersAsync(dto, userName);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("Index", "OfferLetter");
        }

        TempData["ErrorMessage"] = result.Message;

        // Reload dropdowns
        var templatesReload = await _templateService.GetAllTemplatesAsync();
        ViewBag.Templates = templatesReload.Success ? templatesReload.Data : new List<OfferLetterTemplateDto>();

        var departmentsReload = await _masterDataService.GetAllDepartmentsAsync();
        var designationsReload = await _masterDataService.GetAllDesignationsAsync();

        ViewBag.Departments = departmentsReload.Success ? departmentsReload.Data : new List<Application.DTOs.Department.DepartmentDto>();
        ViewBag.Designations = designationsReload.Success ? designationsReload.Data : new List<Application.DTOs.Designation.DesignationDto>();

        return View(dto);
    }
}
