using AutoMapper;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Models;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<EmployeeService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> GetAllEmployeesAsync()
    {
        try
        {
            var employeeRepo = _unitOfWork.Repository<Employee>();
            var employees = await employeeRepo.GetAllAsync();

            // Load navigation properties
            var employeesList = employees.ToList();
            foreach (var emp in employeesList)
            {
                await LoadNavigationPropertiesAsync(emp);
            }

            var employeeDtos = _mapper.Map<IEnumerable<EmployeeDto>>(employeesList);
            return Result<IEnumerable<EmployeeDto>>.SuccessResult(employeeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all employees");
            return Result<IEnumerable<EmployeeDto>>.FailureResult("Error retrieving employees");
        }
    }

    public async Task<Result<EmployeeDto>> GetEmployeeByIdAsync(int id)
    {
        try
        {
            var employeeRepo = _unitOfWork.Repository<Employee>();
            var employee = await employeeRepo.GetByIdAsync(id);

            if (employee == null)
            {
                return Result<EmployeeDto>.FailureResult("Employee not found");
            }

            await LoadNavigationPropertiesAsync(employee);

            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return Result<EmployeeDto>.SuccessResult(employeeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee {EmployeeId}", id);
            return Result<EmployeeDto>.FailureResult("Error retrieving employee");
        }
    }

    public async Task<Result<EmployeeDto>> GetEmployeeByEmailAsync(string email)
    {
        try
        {
            var employeeRepo = _unitOfWork.Repository<Employee>();
            var employees = await employeeRepo.FindAsync(e => e.Email == email);
            var employee = employees.FirstOrDefault();

            if (employee == null)
            {
                return Result<EmployeeDto>.FailureResult("Employee not found");
            }

            await LoadNavigationPropertiesAsync(employee);

            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return Result<EmployeeDto>.SuccessResult(employeeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee by email {Email}", email);
            return Result<EmployeeDto>.FailureResult("Error retrieving employee");
        }
    }

    public async Task<Result<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto dto, string createdBy)
    {
        try
        {
            // Validate unique constraints
            if (!await IsEmployeeCodeUniqueAsync(dto.EmployeeCode))
            {
                return Result<EmployeeDto>.FailureResult("Employee code already exists");
            }

            if (!await IsEmailUniqueAsync(dto.Email))
            {
                return Result<EmployeeDto>.FailureResult("Email already exists");
            }

            var employee = _mapper.Map<Employee>(dto);
            employee.CreatedBy = createdBy;
            employee.CreatedOn = DateTime.UtcNow;

            var employeeRepo = _unitOfWork.Repository<Employee>();
            await employeeRepo.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeCode} created by {CreatedBy}", employee.EmployeeCode, createdBy);

            await LoadNavigationPropertiesAsync(employee);
            var employeeDto = _mapper.Map<EmployeeDto>(employee);

            return Result<EmployeeDto>.SuccessResult(employeeDto, "Employee created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return Result<EmployeeDto>.FailureResult("Error creating employee");
        }
    }

    public async Task<Result<EmployeeDto>> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, string modifiedBy)
    {
        try
        {
            var employeeRepo = _unitOfWork.Repository<Employee>();
            var employee = await employeeRepo.GetByIdAsync(id);

            if (employee == null)
            {
                return Result<EmployeeDto>.FailureResult("Employee not found");
            }

            // Validate unique constraints (excluding current employee)
            if (!await IsEmailUniqueAsync(dto.Email, id))
            {
                return Result<EmployeeDto>.FailureResult("Email already exists");
            }

            _mapper.Map(dto, employee);
            employee.ModifiedBy = modifiedBy;
            employee.ModifiedOn = DateTime.UtcNow;

            employeeRepo.Update(employee);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeId} updated by {ModifiedBy}", employee.Id, modifiedBy);

            await LoadNavigationPropertiesAsync(employee);
            var employeeDto = _mapper.Map<EmployeeDto>(employee);

            return Result<EmployeeDto>.SuccessResult(employeeDto, "Employee updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {EmployeeId}", id);
            return Result<EmployeeDto>.FailureResult("Error updating employee");
        }
    }

    public async Task<Result> DeleteEmployeeAsync(int id)
    {
        try
        {
            var employeeRepo = _unitOfWork.Repository<Employee>();
            var employee = await employeeRepo.GetByIdAsync(id);

            if (employee == null)
            {
                return Result.FailureResult("Employee not found");
            }

            employeeRepo.Remove(employee); // Soft delete
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeId} deleted", id);

            return Result.SuccessResult("Employee deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
            return Result.FailureResult("Error deleting employee");
        }
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> SearchEmployeesAsync(string searchTerm)
    {
        try
        {
            var employeeRepo = _unitOfWork.Repository<Employee>();
            var employees = await employeeRepo.FindAsync(e =>
                e.EmployeeCode.Contains(searchTerm) ||
                e.FirstName.Contains(searchTerm) ||
                e.LastName.Contains(searchTerm) ||
                e.Email.Contains(searchTerm));

            var employeesList = employees.ToList();
            foreach (var emp in employeesList)
            {
                await LoadNavigationPropertiesAsync(emp);
            }

            var employeeDtos = _mapper.Map<IEnumerable<EmployeeDto>>(employeesList);
            return Result<IEnumerable<EmployeeDto>>.SuccessResult(employeeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching employees with term {SearchTerm}", searchTerm);
            return Result<IEnumerable<EmployeeDto>>.FailureResult("Error searching employees");
        }
    }

    public async Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, int? excludeId = null)
    {
        var employeeRepo = _unitOfWork.Repository<Employee>();

        if (excludeId.HasValue)
        {
            return !await employeeRepo.AnyAsync(e => e.EmployeeCode == employeeCode && e.Id != excludeId.Value);
        }

        return !await employeeRepo.AnyAsync(e => e.EmployeeCode == employeeCode);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        var employeeRepo = _unitOfWork.Repository<Employee>();

        if (excludeId.HasValue)
        {
            return !await employeeRepo.AnyAsync(e => e.Email == email && e.Id != excludeId.Value);
        }

        return !await employeeRepo.AnyAsync(e => e.Email == email);
    }

    private async Task LoadNavigationPropertiesAsync(Employee employee)
    {
        var departmentRepo = _unitOfWork.Repository<Department>();
        var designationRepo = _unitOfWork.Repository<Designation>();
        var employeeRepo = _unitOfWork.Repository<Employee>();

        employee.Department = (await departmentRepo.GetByIdAsync(employee.DepartmentId))!;
        employee.Designation = (await designationRepo.GetByIdAsync(employee.DesignationId))!;

        if (employee.ReportingManagerId.HasValue)
        {
            employee.ReportingManager = await employeeRepo.GetByIdAsync(employee.ReportingManagerId.Value);
        }
    }
}
