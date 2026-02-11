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
        var offerLetterResult = await _offerLetterService.GetOfferLetterByIdAsync(id);
        if (!offerLetterResult.Success)
        {
            TempData["ErrorMessage"] = "Offer letter not found";
            return RedirectToAction(nameof(Index));
        }

        var pdfResult = await _offerLetterService.GenerateOfferLetterPdfAsync(id);

        if (!pdfResult.Success)
        {
            TempData["ErrorMessage"] = pdfResult.Message;
            return RedirectToAction(nameof(Index));
        }

        var offerLetter = offerLetterResult.Data!;
        var fileName = $"OfferLetter_{offerLetter.CandidateName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";

        _logger.LogInformation("Generated PDF size: {Size} bytes for Offer Letter {ID}", pdfResult.Data!.Length, id);

        var stream = new MemoryStream(pdfResult.Data!);
        Response.Headers.Append("Content-Disposition", $"attachment; filename={fileName}");
        return File(stream, "application/pdf", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, int status)
    {
        // If status is Accepted (3), redirect to Accept logic
        if ((OfferLetterStatus)status == OfferLetterStatus.Accepted)
        {
            return await Accept(id);
        }

        var result = await _offerLetterService.UpdateOfferLetterStatusAsync(id, (OfferLetterStatus)status, User.Identity!.Name!);

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
    public async Task<IActionResult> Accept(int id)
    {
        try
        {
            var result = await _offerLetterService.AcceptOfferAndCreateEmployeeAsync(id, User.Identity!.Name!);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                _logger.LogWarning("Failed to accept offer {OfferId}: {Message}", id, result.Message);
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = result.Message + $" Employee ID: {result.Data!.Id}";
            _logger.LogInformation("Successfully accepted offer {OfferId}, created employee {EmployeeId}", id, result.Data.Id);

            return RedirectToAction("Details", "Employee", new { area = "Admin", id = result.Data!.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Accept action for offer {OfferId}", id);
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
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
        var templateRepo = _unitOfWork.Repository<Domain.Entities.OfferLetterTemplate>();
        var employeeRepo = _unitOfWork.Repository<Domain.Entities.Employee>();

        var departments = await departmentRepo.FindAsync(d => d.IsActive);
        var designations = await designationRepo.FindAsync(d => d.IsActive);
        var templates = await templateRepo.FindAsync(t => t.IsActive && !t.IsDeleted);
        var employees = await employeeRepo.FindAsync(e => e.Status == EmployeeStatus.Draft && !e.IsDeleted);

        ViewBag.Departments = new SelectList(departments, "Id", "Name");
        ViewBag.Designations = new SelectList(designations, "Id", "Title");
        ViewBag.Templates = new SelectList(templates, "Id", "Name");
        ViewBag.Employees = new SelectList(employees.Select(e => new { 
            Id = e.Id, 
            DisplayText = $"{e.FirstName} {e.LastName} ({e.EmployeeCode})" 
        }), "Id", "DisplayText");
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployeeDetails(int id)
    {
        var employee = await _unitOfWork.Repository<Domain.Entities.Employee>().GetByIdAsync(id);
        if (employee == null) return NotFound();

        return Json(new {
            candidateName = $"{employee.FirstName} {employee.LastName}",
            candidateEmail = employee.Email,
            candidatePhone = employee.Phone,
            candidateAddress = employee.Address,
            departmentId = employee.DepartmentId,
            designationId = employee.DesignationId
        });
    }
}
