using HRMS.Application.DTOs.Salary;
using HRMS.Shared.Models;

namespace HRMS.Application.Interfaces;

public interface ISalaryService
{
    Task<Result<IEnumerable<SalaryDto>>> GetAllSalariesAsync();
    Task<Result<SalaryDto>> GetSalaryByIdAsync(int id);
    Task<Result<SalaryDto>> GetActiveSalaryByEmployeeIdAsync(int employeeId);
    Task<Result<IEnumerable<SalaryDto>>> GetSalaryHistoryByEmployeeIdAsync(int employeeId);
    Task<Result<SalaryDto>> CreateSalaryAsync(CreateSalaryDto dto, string createdBy);
    Task<Result<SalaryDto>> UpdateSalaryAsync(int id, CreateSalaryDto dto, string modifiedBy);
    Task<Result> DeleteSalaryAsync(int id);
}
