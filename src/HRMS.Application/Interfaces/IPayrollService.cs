using HRMS.Application.DTOs.Payroll;
using HRMS.Shared.Models;

namespace HRMS.Application.Interfaces;

public interface IPayrollService
{
    Task<Result<IEnumerable<PayrollDto>>> GetAllPayrollsAsync();
    Task<Result<PayrollDto>> GetPayrollByIdAsync(int id);
    Task<Result<IEnumerable<PayrollDto>>> GetPayrollsByMonthYearAsync(int month, int year);
    Task<Result<IEnumerable<PayrollDto>>> GetPayrollsByEmployeeIdAsync(int employeeId);
    Task<Result<IEnumerable<PayrollDto>>> GeneratePayrollAsync(GeneratePayrollDto dto, string createdBy);
    Task<Result<byte[]>> GeneratePayslipPdfAsync(int payrollId);
    Task<Result> DeletePayrollAsync(int id);
}
