using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,HRAdmin")]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(
        IEmployeeService employeeService,
        IUnitOfWork unitOfWork,
        ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var result = string.IsNullOrWhiteSpace(search)
            ? await _employeeService.GetAllEmployeesAsync()
            : await _employeeService.SearchEmployeesAsync(search);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<EmployeeDto>());
        }

        ViewData["SearchTerm"] = search;
        return View(result.Data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _employeeService.GetEmployeeByIdAsync(id);

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
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeDto model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        var result = await _employeeService.CreateEmployeeAsync(model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error creating employee");
            await PopulateDropdownsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _employeeService.GetEmployeeByIdAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var updateDto = new UpdateEmployeeDto
        {
            Id = result.Data!.Id,
            FirstName = result.Data.FirstName,
            LastName = result.Data.LastName,
            Email = result.Data.Email,
            Phone = result.Data.Phone,
            DateOfBirth = result.Data.DateOfBirth,
            Gender = (int)Enum.Parse<Gender>(result.Data.Gender),
            Address = result.Data.Address,
            DepartmentId = result.Data.DepartmentId,
            DesignationId = result.Data.DesignationId,
            ReportingManagerId = result.Data.ReportingManagerId,
            RelievingDate = result.Data.RelievingDate,
            Status = (int)Enum.Parse<EmployeeStatus>(result.Data.Status)
        };

        await PopulateDropdownsAsync(updateDto.DepartmentId, updateDto.DesignationId, updateDto.ReportingManagerId);
        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateEmployeeDto model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.DepartmentId, model.DesignationId, model.ReportingManagerId);
            return View(model);
        }

        var result = await _employeeService.UpdateEmployeeAsync(model, User.Identity!.Name!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Error updating employee");
            await PopulateDropdownsAsync(model.DepartmentId, model.DesignationId, model.ReportingManagerId);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _employeeService.DeleteEmployeeAsync(id);

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

    private async Task PopulateDropdownsAsync(int? selectedDepartment = null, int? selectedDesignation = null, int? selectedManager = null)
    {
        var departmentRepo = _unitOfWork.Repository<Domain.Entities.Department>();
        var designationRepo = _unitOfWork.Repository<Domain.Entities.Designation>();
        var employeeRepo = _unitOfWork.Repository<Domain.Entities.Employee>();

        var departments = await departmentRepo.FindAsync(d => d.IsActive);
        var designations = await designationRepo.FindAsync(d => d.IsActive);
        var employees = await employeeRepo.GetAllAsync();

        ViewBag.Departments = new SelectList(departments, "Id", "Name", selectedDepartment);
        ViewBag.Designations = new SelectList(designations, "Id", "Title", selectedDesignation);
        ViewBag.Managers = new SelectList(employees.Select(e => new { e.Id, FullName = $"{e.FirstName} {e.LastName}" }), "Id", "FullName", selectedManager);
        ViewBag.Genders = new SelectList(Enum.GetValues(typeof(Gender)).Cast<Gender>().Select(g => new { Id = (int)g, Name = g.ToString() }), "Id", "Name");
        ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(EmployeeStatus)).Cast<EmployeeStatus>().Select(s => new { Id = (int)s, Name = s.ToString() }), "Id", "Name");
    }
}
