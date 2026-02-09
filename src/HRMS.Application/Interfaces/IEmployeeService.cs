using HRMS.Application.DTOs.Employee;
using HRMS.Shared.Models;

namespace HRMS.Application.Interfaces;

public interface IEmployeeService
{
    Task<Result<IEnumerable<EmployeeDto>>> GetAllEmployeesAsync();
    Task<Result<EmployeeDto>> GetEmployeeByIdAsync(int id);
    Task<Result<EmployeeDto>> GetEmployeeByEmailAsync(string email);
    Task<Result<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto employeeDto, string createdBy);
    Task<Result<EmployeeDto>> UpdateEmployeeAsync(int id, UpdateEmployeeDto employeeDto, string modifiedBy);
    Task<Result> DeleteEmployeeAsync(int id);
    Task<Result<IEnumerable<EmployeeDto>>> SearchEmployeesAsync(string searchTerm);
    Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, int? excludeId = null);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
}
