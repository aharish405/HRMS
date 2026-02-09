using HRMS.Application.DTOs.Department;
using HRMS.Application.DTOs.Designation;
using HRMS.Shared.Models;

namespace HRMS.Application.Interfaces;

public interface IMasterDataService
{
    // Department operations
    Task<Result<IEnumerable<DepartmentDto>>> GetAllDepartmentsAsync();
    Task<Result<DepartmentDto>> GetDepartmentByIdAsync(int id);
    Task<Result<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto dto, string createdBy);
    Task<Result<DepartmentDto>> UpdateDepartmentAsync(int id, CreateDepartmentDto dto, string modifiedBy);
    Task<Result> DeleteDepartmentAsync(int id);

    // Designation operations
    Task<Result<IEnumerable<DesignationDto>>> GetAllDesignationsAsync();
    Task<Result<DesignationDto>> GetDesignationByIdAsync(int id);
    Task<Result<DesignationDto>> CreateDesignationAsync(CreateDesignationDto dto, string createdBy);
    Task<Result<DesignationDto>> UpdateDesignationAsync(int id, CreateDesignationDto dto, string modifiedBy);
    Task<Result> DeleteDesignationAsync(int id);
}
