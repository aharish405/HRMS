using AutoMapper;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Models;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.IO.Font.Constants;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PayrollService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<PayrollDto>>> GetAllPayrollsAsync()
    {
        try
        {
            var payrollRepo = _unitOfWork.Repository<Payroll>();
            var payrolls = await payrollRepo.GetAllAsync();

            var payrollsList = payrolls.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToList();
            foreach (var payroll in payrollsList)
            {
                await LoadNavigationPropertiesAsync(payroll);
            }

            var payrollDtos = _mapper.Map<IEnumerable<PayrollDto>>(payrollsList);
            return Result<IEnumerable<PayrollDto>>.SuccessResult(payrollDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all payrolls");
            return Result<IEnumerable<PayrollDto>>.FailureResult("Error retrieving payrolls");
        }
    }

    public async Task<Result<PayrollDto>> GetPayrollByIdAsync(int id)
    {
        try
        {
            var payrollRepo = _unitOfWork.Repository<Payroll>();
            var payroll = await payrollRepo.GetByIdAsync(id);

            if (payroll == null)
            {
                return Result<PayrollDto>.FailureResult("Payroll not found");
            }

            await LoadNavigationPropertiesAsync(payroll);

            var payrollDto = _mapper.Map<PayrollDto>(payroll);
            return Result<PayrollDto>.SuccessResult(payrollDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payroll {PayrollId}", id);
            return Result<PayrollDto>.FailureResult("Error retrieving payroll");
        }
    }

    public async Task<Result<IEnumerable<PayrollDto>>> GetPayrollsByMonthYearAsync(int month, int year)
    {
        try
        {
            var payrollRepo = _unitOfWork.Repository<Payroll>();
            var payrolls = await payrollRepo.FindAsync(p => p.Month == month && p.Year == year);

            var payrollsList = payrolls.ToList();
            foreach (var payroll in payrollsList)
            {
                await LoadNavigationPropertiesAsync(payroll);
            }

            var payrollDtos = _mapper.Map<IEnumerable<PayrollDto>>(payrollsList);
            return Result<IEnumerable<PayrollDto>>.SuccessResult(payrollDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payrolls for {Month}/{Year}", month, year);
            return Result<IEnumerable<PayrollDto>>.FailureResult("Error retrieving payrolls");
        }
    }

    public async Task<Result<IEnumerable<PayrollDto>>> GetPayrollsByEmployeeIdAsync(int employeeId)
    {
        try
        {
            var payrollRepo = _unitOfWork.Repository<Payroll>();
            var payrolls = await payrollRepo.FindAsync(p => p.EmployeeId == employeeId);

            var payrollsList = payrolls.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToList();
            foreach (var payroll in payrollsList)
            {
                await LoadNavigationPropertiesAsync(payroll);
            }

            var payrollDtos = _mapper.Map<IEnumerable<PayrollDto>>(payrollsList);
            return Result<IEnumerable<PayrollDto>>.SuccessResult(payrollDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payrolls for employee {EmployeeId}", employeeId);
            return Result<IEnumerable<PayrollDto>>.FailureResult("Error retrieving payrolls");
        }
    }

    public async Task<Result<IEnumerable<PayrollDto>>> GeneratePayrollAsync(GeneratePayrollDto dto, string createdBy)
    {
        try
        {
            var employeeRepo = _unitOfWork.Repository<Employee>();
            var salaryRepo = _unitOfWork.Repository<Salary>();
            var payrollRepo = _unitOfWork.Repository<Payroll>();

            // Get employees to process
            IEnumerable<Employee> employees;
            if (dto.EmployeeId.HasValue)
            {
                var employee = await employeeRepo.GetByIdAsync(dto.EmployeeId.Value);
                if (employee == null)
                {
                    return Result<IEnumerable<PayrollDto>>.FailureResult("Employee not found");
                }
                employees = new[] { employee };
            }
            else
            {
                employees = await employeeRepo.FindAsync(e => e.Status == EmployeeStatus.Active);
            }

            var generatedPayrolls = new List<Payroll>();
            var daysInMonth = DateTime.DaysInMonth(dto.Year, dto.Month);
            var monthStart = new DateTime(dto.Year, dto.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            foreach (var employee in employees)
            {
                // Check if payroll already exists
                var existingPayroll = await payrollRepo.FindAsync(p =>
                    p.EmployeeId == employee.Id && p.Month == dto.Month && p.Year == dto.Year);

                if (existingPayroll.Any())
                {
                    _logger.LogWarning("Payroll already exists for employee {EmployeeId} for {Month}/{Year}",
                        employee.Id, dto.Month, dto.Year);
                    continue;
                }

                // Get active salary
                var activeSalaries = await salaryRepo.FindAsync(s => s.EmployeeId == employee.Id && s.IsActive);
                var activeSalary = activeSalaries.FirstOrDefault();

                if (activeSalary == null)
                {
                    _logger.LogWarning("No active salary found for employee {EmployeeId}", employee.Id);
                    continue;
                }

                // --- Pro-Rata Calculation Logic ---
                var paidDays = daysInMonth; // Default to full month
                var isProRated = false;
                DateTime? joiningDateRef = null;

                if (employee.JoiningDate.Year == dto.Year && employee.JoiningDate.Month == dto.Month)
                {
                    // Joined during this month
                    paidDays = (monthEnd - employee.JoiningDate).Days + 1;
                    paidDays = Math.Max(0, paidDays); // Validated non-negative
                    isProRated = paidDays < daysInMonth;
                    joiningDateRef = employee.JoiningDate;
                }
                else if (employee.JoiningDate > monthEnd)
                {
                    // Future joining date (shouldn't happen for active, but safe guard)
                    paidDays = 0;
                    isProRated = true;
                }

                // Calculate ratio
                var ratio = (decimal)paidDays / daysInMonth;
                
                // Attendance (Simplified: Assumes present for all paid days)
                var workingDays = daysInMonth; // Standardize to calendar days for this calculation
                var presentDays = paidDays;
                var leaveDays = 0; 
                var absentDays = workingDays - presentDays - leaveDays;

                // Create payroll
                var payroll = new Payroll
                {
                    EmployeeId = employee.Id,
                    Month = dto.Month,
                    Year = dto.Year,
                    
                    // Stats
                    WorkingDays = workingDays,
                    TotalCalendarDays = daysInMonth,
                    PaidDays = paidDays,
                    IsProRated = isProRated,
                    JoiningDate = joiningDateRef,
                    PresentDays = presentDays,
                    LeaveDays = leaveDays,
                    AbsentDays = absentDays,

                    // Earnings (Pro-Rated)
                    BasicSalary = Math.Round(activeSalary.BasicSalary * ratio, 2),
                    HRA = Math.Round(activeSalary.HRA * ratio, 2),
                    ConveyanceAllowance = Math.Round(activeSalary.ConveyanceAllowance * ratio, 2),
                    MedicalAllowance = Math.Round(activeSalary.MedicalAllowance * ratio, 2),
                    SpecialAllowance = Math.Round(activeSalary.SpecialAllowance * ratio, 2),
                    OtherAllowances = Math.Round(activeSalary.OtherAllowances * ratio, 2),
                    
                    // Deductions (Pro-Rated)
                    PF = Math.Round(activeSalary.PF * ratio, 2),
                    ESI = Math.Round(activeSalary.ESI * ratio, 2),
                    ProfessionalTax = Math.Round(activeSalary.ProfessionalTax * ratio, 2),
                    TDS = Math.Round(activeSalary.TDS * ratio, 2),
                    OtherDeductions = Math.Round(activeSalary.OtherDeductions * ratio, 2),
                    
                    IsProcessed = true,
                    ProcessedOn = DateTime.UtcNow,
                    ProcessedBy = createdBy,
                    CreatedBy = createdBy,
                    CreatedOn = DateTime.UtcNow
                };

                // Recalculate Totals based on pro-rated components
                payroll.GrossSalary = payroll.BasicSalary + payroll.HRA + payroll.ConveyanceAllowance + 
                                      payroll.MedicalAllowance + payroll.SpecialAllowance + payroll.OtherAllowances;
                
                payroll.TotalDeductions = payroll.PF + payroll.ESI + payroll.ProfessionalTax + 
                                          payroll.TDS + payroll.OtherDeductions;
                
                payroll.NetSalary = payroll.GrossSalary - payroll.TotalDeductions;
                
                // Store Per-Day Reference (based on Master Salary / Calendar Days)
                payroll.PerDaySalary = daysInMonth > 0 ? Math.Round(activeSalary.GrossSalary / daysInMonth, 2) : 0;


                await payrollRepo.AddAsync(payroll);
                generatedPayrolls.Add(payroll);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Generated {Count} payroll records for {Month}/{Year} by {CreatedBy}",
                generatedPayrolls.Count, dto.Month, dto.Year, createdBy);

            // Load navigation properties
            foreach (var payroll in generatedPayrolls)
            {
                await LoadNavigationPropertiesAsync(payroll);
            }

            var payrollDtos = _mapper.Map<IEnumerable<PayrollDto>>(generatedPayrolls);
            return Result<IEnumerable<PayrollDto>>.SuccessResult(payrollDtos,
                $"Successfully generated {generatedPayrolls.Count} payroll record(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payroll");
            return Result<IEnumerable<PayrollDto>>.FailureResult("Error generating payroll");
        }
    }

    public async Task<Result<byte[]>> GeneratePayslipPdfAsync(int payrollId)
    {
        try
        {
            var payrollRepo = _unitOfWork.Repository<Payroll>();
            var payroll = await payrollRepo.GetByIdAsync(payrollId);

            if (payroll == null)
            {
                return Result<byte[]>.FailureResult("Payroll not found");
            }

            await LoadNavigationPropertiesAsync(payroll);

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add company header
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var header = new Paragraph("WorkAxis HRMS")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetFont(boldFont);
            document.Add(header);

            var subHeader = new Paragraph("Payslip")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(16);
            document.Add(subHeader);

            var monthYear = new Paragraph($"For the month of {GetMonthName(payroll.Month)} {payroll.Year}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12);
            document.Add(monthYear);

            document.Add(new Paragraph("\n"));

            // Employee Details Table
            var empTable = new Table(4);
            empTable.SetWidth(UnitValue.CreatePercentValue(100));

            AddCell(empTable, "Employee Code:", true);
            AddCell(empTable, payroll.Employee.EmployeeCode, false);
            AddCell(empTable, "Employee Name:", true);
            AddCell(empTable, payroll.Employee.FullName, false);

            AddCell(empTable, "Department:", true);
            AddCell(empTable, payroll.Employee.Department.Name, false);
            AddCell(empTable, "Designation:", true);
            AddCell(empTable, payroll.Employee.Designation.Title, false);

            if (payroll.IsProRated && payroll.JoiningDate.HasValue)
            {
                AddCell(empTable, "Joining Date:", true);
                AddCell(empTable, payroll.JoiningDate.Value.ToString("dd MMM yyyy"), false);
                AddCell(empTable, "Remarks:", true);
                AddCell(empTable, "Pro-Rated Salary", false);
            }

            AddCell(empTable, "Calendar Days:", true);
            AddCell(empTable, payroll.TotalCalendarDays.ToString(), false);
            AddCell(empTable, "Paid Days:", true);
            AddCell(empTable, payroll.PaidDays.ToString(), false);

            document.Add(empTable);
            document.Add(new Paragraph("\n"));

            // Earnings and Deductions Table
            var salaryTable = new Table(4);
            salaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Headers
            AddHeaderCell(salaryTable, "Earnings");
            AddHeaderCell(salaryTable, "Amount (₹)");
            AddHeaderCell(salaryTable, "Deductions");
            AddHeaderCell(salaryTable, "Amount (₹)");

            // Rows
            AddSalaryRow(salaryTable, "Basic Salary", payroll.BasicSalary, "PF", payroll.PF);
            AddSalaryRow(salaryTable, "HRA", payroll.HRA, "ESI", payroll.ESI);
            AddSalaryRow(salaryTable, "Conveyance", payroll.ConveyanceAllowance, "Professional Tax", payroll.ProfessionalTax);
            AddSalaryRow(salaryTable, "Medical Allowance", payroll.MedicalAllowance, "TDS", payroll.TDS);
            AddSalaryRow(salaryTable, "Special Allowance", payroll.SpecialAllowance, "Other Deductions", payroll.OtherDeductions);
            AddSalaryRow(salaryTable, "Other Allowances", payroll.OtherAllowances, "", 0);

            // Totals
            AddTotalRow(salaryTable, "Gross Salary", payroll.GrossSalary, "Total Deductions", payroll.TotalDeductions);

            document.Add(salaryTable);
            document.Add(new Paragraph("\n"));

            // Net Salary
            var netSalaryPara = new Paragraph($"Net Salary: ₹{payroll.NetSalary:N2}")
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(14)
                .SetFont(boldFont);
            document.Add(netSalaryPara);

            document.Add(new Paragraph("\n\n"));

            // Footer
            var italicFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);
            var footer = new Paragraph("This is a computer-generated payslip and does not require a signature.")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(8)
                .SetFont(italicFont);
            document.Add(footer);

            document.Close();

            _logger.LogInformation("Generated payslip PDF for payroll {PayrollId}", payrollId);

            return Result<byte[]>.SuccessResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payslip PDF for payroll {PayrollId}", payrollId);
            return Result<byte[]>.FailureResult("Error generating payslip PDF");
        }
    }

    public async Task<Result> DeletePayrollAsync(int id)
    {
        try
        {
            var payrollRepo = _unitOfWork.Repository<Payroll>();
            var payroll = await payrollRepo.GetByIdAsync(id);

            if (payroll == null)
            {
                return Result.FailureResult("Payroll not found");
            }

            payrollRepo.Remove(payroll);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Payroll {PayrollId} deleted", id);

            return Result.SuccessResult("Payroll deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payroll {PayrollId}", id);
            return Result.FailureResult("Error deleting payroll");
        }
    }

    private async Task LoadNavigationPropertiesAsync(Payroll payroll)
    {
        var employeeRepo = _unitOfWork.Repository<Employee>();
        var departmentRepo = _unitOfWork.Repository<Department>();
        var designationRepo = _unitOfWork.Repository<Designation>();

        payroll.Employee = (await employeeRepo.GetByIdAsync(payroll.EmployeeId))!;
        payroll.Employee.Department = (await departmentRepo.GetByIdAsync(payroll.Employee.DepartmentId))!;
        payroll.Employee.Designation = (await designationRepo.GetByIdAsync(payroll.Employee.DesignationId))!;
    }

    private static string GetMonthName(int month)
    {
        return new DateTime(2000, month, 1).ToString("MMMM");
    }

    // PDF Helper Methods
    private static void AddCell(Table table, string text, bool isBold)
    {
        var cell = new Cell().Add(new Paragraph(text));
        if (isBold)
        {
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            cell.SetFont(boldFont);
        }
        table.AddCell(cell);
    }

    private static void AddHeaderCell(Table table, string text)
    {
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var cell = new Cell().Add(new Paragraph(text))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetFont(boldFont)
            .SetTextAlignment(TextAlignment.CENTER);
        table.AddCell(cell);
    }

    private static void AddSalaryRow(Table table, string earning, decimal earningAmount, string deduction, decimal deductionAmount)
    {
        table.AddCell(new Cell().Add(new Paragraph(earning)));
        table.AddCell(new Cell().Add(new Paragraph(earningAmount.ToString("N2"))).SetTextAlignment(TextAlignment.RIGHT));
        table.AddCell(new Cell().Add(new Paragraph(deduction)));
        table.AddCell(new Cell().Add(new Paragraph(deductionAmount > 0 ? deductionAmount.ToString("N2") : "-")).SetTextAlignment(TextAlignment.RIGHT));
    }

    private static void AddTotalRow(Table table, string earning, decimal earningAmount, string deduction, decimal deductionAmount)
    {
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        table.AddCell(new Cell().Add(new Paragraph(earning).SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
        table.AddCell(new Cell().Add(new Paragraph(earningAmount.ToString("N2")).SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.RIGHT));
        table.AddCell(new Cell().Add(new Paragraph(deduction).SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
        table.AddCell(new Cell().Add(new Paragraph(deductionAmount.ToString("N2")).SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.RIGHT));
    }
}
