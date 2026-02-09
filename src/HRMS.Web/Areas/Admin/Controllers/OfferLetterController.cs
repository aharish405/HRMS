using HRMS.Application.DTOs.OfferLetter;
using HRMS.Application.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,HRAdmin")]
public class OfferLetterController : Controller
{
    private readonly IOfferLetterService _offerLetterService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OfferLetterController> _logger;

    public OfferLetterController(
        IOfferLetterService offerLetterService,
        IUnitOfWork unitOfWork,
        ILogger<OfferLetterController> logger)
    {
        _offerLetterService = offerLetterService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _offerLetterService.GetAllOfferLettersAsync();

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<OfferLetterDto>());
        }

        return View(result.Data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _offerLetterService.GetOfferLetterByIdAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();

        var model = new CreateOfferLetterDto
        {
            JoiningDate = DateTime.Today.AddDays(30)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOfferLetterDto model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        var result = await _offerLetterService.CreateOfferLetterAsync(model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error creating offer letter");
            await PopulateDropdownsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DownloadPdf(int id)
    {
        var result = await _offerLetterService.GenerateOfferLetterPdfAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var offerLetterResult = await _offerLetterService.GetOfferLetterByIdAsync(id);
        var fileName = $"OfferLetter_{offerLetterResult.Data!.CandidateName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";

        return File(result.Data!, "application/pdf", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OfferLetterStatus status)
    {
        var result = await _offerLetterService.UpdateOfferLetterStatusAsync(id, status, User.Identity!.Name!);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _offerLetterService.DeleteOfferLetterAsync(id);

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

    private async Task PopulateDropdownsAsync()
    {
        var departmentRepo = _unitOfWork.Repository<Domain.Entities.Department>();
        var designationRepo = _unitOfWork.Repository<Domain.Entities.Designation>();

        var departments = await departmentRepo.FindAsync(d => d.IsActive);
        var designations = await designationRepo.FindAsync(d => d.IsActive);

        ViewBag.Departments = new SelectList(departments, "Id", "Name");
        ViewBag.Designations = new SelectList(designations, "Id", "Title");
    }
}
