using AutoMapper;
using HRMS.Application.DTOs.Salary;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Models;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services;

public class SalaryService : ISalaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SalaryService> _logger;

    public SalaryService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SalaryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<SalaryDto>>> GetAllSalariesAsync()
    {
        try
        {
            var salaryRepo = _unitOfWork.Repository<Salary>();
            var salaries = await salaryRepo.GetAllAsync();

            var salariesList = salaries.ToList();
            foreach (var salary in salariesList)
            {
                await LoadEmployeeAsync(salary);
            }

            var salaryDtos = _mapper.Map<IEnumerable<SalaryDto>>(salariesList);
            return Result<IEnumerable<SalaryDto>>.SuccessResult(salaryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all salaries");
            return Result<IEnumerable<SalaryDto>>.FailureResult("Error retrieving salaries");
        }
    }

    public async Task<Result<SalaryDto>> GetSalaryByIdAsync(int id)
    {
        try
        {
            var salaryRepo = _unitOfWork.Repository<Salary>();
            var salary = await salaryRepo.GetByIdAsync(id);

            if (salary == null)
            {
                return Result<SalaryDto>.FailureResult("Salary not found");
            }

            await LoadEmployeeAsync(salary);

            var salaryDto = _mapper.Map<SalaryDto>(salary);
            return Result<SalaryDto>.SuccessResult(salaryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting salary {SalaryId}", id);
            return Result<SalaryDto>.FailureResult("Error retrieving salary");
        }
    }

    public async Task<Result<SalaryDto>> GetActiveSalaryByEmployeeIdAsync(int employeeId)
    {
        try
        {
            var salaryRepo = _unitOfWork.Repository<Salary>();
            var salaries = await salaryRepo.FindAsync(s => s.EmployeeId == employeeId && s.IsActive);

            var salary = salaries.FirstOrDefault();
            if (salary == null)
            {
                return Result<SalaryDto>.FailureResult("No active salary found for employee");
            }

            await LoadEmployeeAsync(salary);

            var salaryDto = _mapper.Map<SalaryDto>(salary);
            return Result<SalaryDto>.SuccessResult(salaryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active salary for employee {EmployeeId}", employeeId);
            return Result<SalaryDto>.FailureResult("Error retrieving salary");
        }
    }

    public async Task<Result<IEnumerable<SalaryDto>>> GetSalaryHistoryByEmployeeIdAsync(int employeeId)
    {
        try
        {
            var salaryRepo = _unitOfWork.Repository<Salary>();
            var salaries = await salaryRepo.FindAsync(s => s.EmployeeId == employeeId);

            var salariesList = salaries.OrderByDescending(s => s.EffectiveFrom).ToList();
            foreach (var salary in salariesList)
            {
                await LoadEmployeeAsync(salary);
            }

            var salaryDtos = _mapper.Map<IEnumerable<SalaryDto>>(salariesList);
            return Result<IEnumerable<SalaryDto>>.SuccessResult(salaryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting salary history for employee {EmployeeId}", employeeId);
            return Result<IEnumerable<SalaryDto>>.FailureResult("Error retrieving salary history");
        }
    }

    public async Task<Result<SalaryDto>> CreateSalaryAsync(CreateSalaryDto dto, string createdBy)
    {
        try
        {
            var employeeRepo = _unitOfWork.Repository<Employee>();
            var employee = await employeeRepo.GetByIdAsync(dto.EmployeeId);

            if (employee == null)
            {
                return Result<SalaryDto>.FailureResult("Employee not found");
            }

            // Deactivate existing active salaries for this employee
            var salaryRepo = _unitOfWork.Repository<Salary>();
            var existingSalaries = await salaryRepo.FindAsync(s => s.EmployeeId == dto.EmployeeId && s.IsActive);

            foreach (var existingSalary in existingSalaries)
            {
                existingSalary.IsActive = false;
                existingSalary.EffectiveTo = dto.EffectiveFrom.AddDays(-1);
                salaryRepo.Update(existingSalary);
            }

            var salary = _mapper.Map<Salary>(dto);
            salary.CreatedBy = createdBy;
            salary.CreatedOn = DateTime.UtcNow;
            salary.IsActive = true;

            // Calculate totals
            CalculateSalaryTotals(salary);

            await salaryRepo.AddAsync(salary);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Salary created for employee {EmployeeId} by {CreatedBy}", dto.EmployeeId, createdBy);

            await LoadEmployeeAsync(salary);
            var salaryDto = _mapper.Map<SalaryDto>(salary);

            return Result<SalaryDto>.SuccessResult(salaryDto, "Salary created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating salary");
            return Result<SalaryDto>.FailureResult("Error creating salary");
        }
    }

    public async Task<Result<SalaryDto>> UpdateSalaryAsync(int id, CreateSalaryDto dto, string modifiedBy)
    {
        try
        {
            var salaryRepo = _unitOfWork.Repository<Salary>();
            var salary = await salaryRepo.GetByIdAsync(id);

            if (salary == null)
            {
                return Result<SalaryDto>.FailureResult("Salary not found");
            }

            _mapper.Map(dto, salary);
            salary.ModifiedBy = modifiedBy;
            salary.ModifiedOn = DateTime.UtcNow;

            // Recalculate totals
            CalculateSalaryTotals(salary);

            salaryRepo.Update(salary);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Salary {SalaryId} updated by {ModifiedBy}", id, modifiedBy);

            await LoadEmployeeAsync(salary);
            var salaryDto = _mapper.Map<SalaryDto>(salary);

            return Result<SalaryDto>.SuccessResult(salaryDto, "Salary updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating salary {SalaryId}", id);
            return Result<SalaryDto>.FailureResult("Error updating salary");
        }
    }

    public async Task<Result> DeleteSalaryAsync(int id)
    {
        try
        {
            var salaryRepo = _unitOfWork.Repository<Salary>();
            var salary = await salaryRepo.GetByIdAsync(id);

            if (salary == null)
            {
                return Result.FailureResult("Salary not found");
            }

            salaryRepo.Remove(salary);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Salary {SalaryId} deleted", id);

            return Result.SuccessResult("Salary deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting salary {SalaryId}", id);
            return Result.FailureResult("Error deleting salary");
        }
    }

    private void CalculateSalaryTotals(Salary salary)
    {
        // Calculate Gross Salary (all allowances)
        salary.GrossSalary = salary.BasicSalary + salary.HRA + salary.ConveyanceAllowance +
                            salary.MedicalAllowance + salary.SpecialAllowance + salary.OtherAllowances;

        // Calculate Total Deductions
        salary.TotalDeductions = salary.PF + salary.ESI + salary.ProfessionalTax +
                                salary.TDS + salary.OtherDeductions;

        // Calculate Net Salary
        salary.NetSalary = salary.GrossSalary - salary.TotalDeductions;

        // Calculate CTC (Gross + Employer contributions - typically PF employer contribution)
        // For simplicity, CTC = Gross + PF (employer contribution assumed equal to employee PF)
        salary.CTC = salary.GrossSalary + salary.PF;
    }

    private async Task LoadEmployeeAsync(Salary salary)
    {
        var employeeRepo = _unitOfWork.Repository<Employee>();
        salary.Employee = (await employeeRepo.GetByIdAsync(salary.EmployeeId))!;
    }
}
