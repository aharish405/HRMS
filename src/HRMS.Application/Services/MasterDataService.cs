using AutoMapper;
using HRMS.Application.DTOs.Department;
using HRMS.Application.DTOs.Designation;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Models;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services;

public class MasterDataService : IMasterDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MasterDataService> _logger;

    public MasterDataService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MasterDataService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region Department Operations

    public async Task<Result<IEnumerable<DepartmentDto>>> GetAllDepartmentsAsync()
    {
        try
        {
            var departmentRepo = _unitOfWork.Repository<Department>();
            var departments = await departmentRepo.GetAllAsync();

            var departmentDtos = _mapper.Map<IEnumerable<DepartmentDto>>(departments);
            return Result<IEnumerable<DepartmentDto>>.SuccessResult(departmentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all departments");
            return Result<IEnumerable<DepartmentDto>>.FailureResult("Error retrieving departments");
        }
    }

    public async Task<Result<DepartmentDto>> GetDepartmentByIdAsync(int id)
    {
        try
        {
            var departmentRepo = _unitOfWork.Repository<Department>();
            var department = await departmentRepo.GetByIdAsync(id);

            if (department == null)
            {
                return Result<DepartmentDto>.FailureResult("Department not found");
            }

            var departmentDto = _mapper.Map<DepartmentDto>(department);
            return Result<DepartmentDto>.SuccessResult(departmentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department {DepartmentId}", id);
            return Result<DepartmentDto>.FailureResult("Error retrieving department");
        }
    }

    public async Task<Result<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto dto, string createdBy)
    {
        try
        {
            var departmentRepo = _unitOfWork.Repository<Department>();

            // Check if code already exists
            if (await departmentRepo.AnyAsync(d => d.Code == dto.Code))
            {
                return Result<DepartmentDto>.FailureResult("Department code already exists");
            }

            var department = _mapper.Map<Department>(dto);
            department.CreatedBy = createdBy;
            department.CreatedOn = DateTime.UtcNow;

            await departmentRepo.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Department {DepartmentCode} created by {CreatedBy}", department.Code, createdBy);

            var departmentDto = _mapper.Map<DepartmentDto>(department);
            return Result<DepartmentDto>.SuccessResult(departmentDto, "Department created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return Result<DepartmentDto>.FailureResult("Error creating department");
        }
    }

    public async Task<Result<DepartmentDto>> UpdateDepartmentAsync(int id, CreateDepartmentDto dto, string modifiedBy)
    {
        try
        {
            var departmentRepo = _unitOfWork.Repository<Department>();
            var department = await departmentRepo.GetByIdAsync(id);

            if (department == null)
            {
                return Result<DepartmentDto>.FailureResult("Department not found");
            }

            // Check if code already exists (excluding current department)
            if (await departmentRepo.AnyAsync(d => d.Code == dto.Code && d.Id != id))
            {
                return Result<DepartmentDto>.FailureResult("Department code already exists");
            }

            _mapper.Map(dto, department);
            department.ModifiedBy = modifiedBy;
            department.ModifiedOn = DateTime.UtcNow;

            departmentRepo.Update(department);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Department {DepartmentId} updated by {ModifiedBy}", id, modifiedBy);

            var departmentDto = _mapper.Map<DepartmentDto>(department);
            return Result<DepartmentDto>.SuccessResult(departmentDto, "Department updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {DepartmentId}", id);
            return Result<DepartmentDto>.FailureResult("Error updating department");
        }
    }

    public async Task<Result> DeleteDepartmentAsync(int id)
    {
        try
        {
            var departmentRepo = _unitOfWork.Repository<Department>();
            var department = await departmentRepo.GetByIdAsync(id);

            if (department == null)
            {
                return Result.FailureResult("Department not found");
            }

            // Check if department has employees
            var employeeRepo = _unitOfWork.Repository<Employee>();
            if (await employeeRepo.AnyAsync(e => e.DepartmentId == id))
            {
                return Result.FailureResult("Cannot delete department with assigned employees");
            }

            departmentRepo.Remove(department);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Department {DepartmentId} deleted", id);

            return Result.SuccessResult("Department deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {DepartmentId}", id);
            return Result.FailureResult("Error deleting department");
        }
    }

    #endregion

    #region Designation Operations

    public async Task<Result<IEnumerable<DesignationDto>>> GetAllDesignationsAsync()
    {
        try
        {
            var designationRepo = _unitOfWork.Repository<Designation>();
            var designations = await designationRepo.GetAllAsync();

            var designationDtos = _mapper.Map<IEnumerable<DesignationDto>>(designations);
            return Result<IEnumerable<DesignationDto>>.SuccessResult(designationDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all designations");
            return Result<IEnumerable<DesignationDto>>.FailureResult("Error retrieving designations");
        }
    }

    public async Task<Result<DesignationDto>> GetDesignationByIdAsync(int id)
    {
        try
        {
            var designationRepo = _unitOfWork.Repository<Designation>();
            var designation = await designationRepo.GetByIdAsync(id);

            if (designation == null)
            {
                return Result<DesignationDto>.FailureResult("Designation not found");
            }

            var designationDto = _mapper.Map<DesignationDto>(designation);
            return Result<DesignationDto>.SuccessResult(designationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting designation {DesignationId}", id);
            return Result<DesignationDto>.FailureResult("Error retrieving designation");
        }
    }

    public async Task<Result<DesignationDto>> CreateDesignationAsync(CreateDesignationDto dto, string createdBy)
    {
        try
        {
            var designationRepo = _unitOfWork.Repository<Designation>();

            // Check if code already exists
            if (await designationRepo.AnyAsync(d => d.Code == dto.Code))
            {
                return Result<DesignationDto>.FailureResult("Designation code already exists");
            }

            var designation = _mapper.Map<Designation>(dto);
            designation.CreatedBy = createdBy;
            designation.CreatedOn = DateTime.UtcNow;

            await designationRepo.AddAsync(designation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Designation {DesignationCode} created by {CreatedBy}", designation.Code, createdBy);

            var designationDto = _mapper.Map<DesignationDto>(designation);
            return Result<DesignationDto>.SuccessResult(designationDto, "Designation created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating designation");
            return Result<DesignationDto>.FailureResult("Error creating designation");
        }
    }

    public async Task<Result<DesignationDto>> UpdateDesignationAsync(int id, CreateDesignationDto dto, string modifiedBy)
    {
        try
        {
            var designationRepo = _unitOfWork.Repository<Designation>();
            var designation = await designationRepo.GetByIdAsync(id);

            if (designation == null)
            {
                return Result<DesignationDto>.FailureResult("Designation not found");
            }

            // Check if code already exists (excluding current designation)
            if (await designationRepo.AnyAsync(d => d.Code == dto.Code && d.Id != id))
            {
                return Result<DesignationDto>.FailureResult("Designation code already exists");
            }

            _mapper.Map(dto, designation);
            designation.ModifiedBy = modifiedBy;
            designation.ModifiedOn = DateTime.UtcNow;

            designationRepo.Update(designation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Designation {DesignationId} updated by {ModifiedBy}", id, modifiedBy);

            var designationDto = _mapper.Map<DesignationDto>(designation);
            return Result<DesignationDto>.SuccessResult(designationDto, "Designation updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating designation {DesignationId}", id);
            return Result<DesignationDto>.FailureResult("Error updating designation");
        }
    }

    public async Task<Result> DeleteDesignationAsync(int id)
    {
        try
        {
            var designationRepo = _unitOfWork.Repository<Designation>();
            var designation = await designationRepo.GetByIdAsync(id);

            if (designation == null)
            {
                return Result.FailureResult("Designation not found");
            }

            // Check if designation has employees
            var employeeRepo = _unitOfWork.Repository<Employee>();
            if (await employeeRepo.AnyAsync(e => e.DesignationId == id))
            {
                return Result.FailureResult("Cannot delete designation with assigned employees");
            }

            designationRepo.Remove(designation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Designation {DesignationId} deleted", id);

            return Result.SuccessResult("Designation deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting designation {DesignationId}", id);
            return Result.FailureResult("Error deleting designation");
        }
    }

    #endregion
}
