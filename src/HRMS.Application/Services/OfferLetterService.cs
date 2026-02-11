using AutoMapper;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.DTOs.OfferLetter;
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
using iText.Html2pdf;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services;

public class OfferLetterService : IOfferLetterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OfferLetterService> _logger;

    public OfferLetterService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OfferLetterService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<OfferLetterDto>>> GetAllOfferLettersAsync()
    {
        try
        {
            var offerLetterRepo = _unitOfWork.Repository<OfferLetter>();
            var offerLetters = await offerLetterRepo.GetAllAsync();

            var offerLettersList = offerLetters.OrderByDescending(o => o.GeneratedOn).ToList();
            foreach (var offerLetter in offerLettersList)
            {
                await LoadNavigationPropertiesAsync(offerLetter);
            }

            var offerLetterDtos = _mapper.Map<IEnumerable<OfferLetterDto>>(offerLettersList);
            return Result<IEnumerable<OfferLetterDto>>.SuccessResult(offerLetterDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all offer letters");
            return Result<IEnumerable<OfferLetterDto>>.FailureResult("Error retrieving offer letters");
        }
    }

    public async Task<Result<OfferLetterDto>> GetOfferLetterByIdAsync(int id)
    {
        try
        {
            var offerLetterRepo = _unitOfWork.Repository<OfferLetter>();
            var offerLetter = await offerLetterRepo.GetByIdAsync(id);

            if (offerLetter == null)
            {
                return Result<OfferLetterDto>.FailureResult("Offer letter not found");
            }

            await LoadNavigationPropertiesAsync(offerLetter);

            var offerLetterDto = _mapper.Map<OfferLetterDto>(offerLetter);
            return Result<OfferLetterDto>.SuccessResult(offerLetterDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting offer letter {OfferLetterId}", id);
            return Result<OfferLetterDto>.FailureResult("Error retrieving offer letter");
        }
    }

    public async Task<Result<OfferLetterDto>> CreateOfferLetterAsync(CreateOfferLetterDto dto, string createdBy)
    {
        try
        {
            // Link to Employee if provided
            if (dto.EmployeeId.HasValue)
            {
                var employeeRepo = _unitOfWork.Repository<Employee>();
                var employee = await employeeRepo.GetByIdAsync(dto.EmployeeId.Value);
                
                if (employee == null)
                    return Result<OfferLetterDto>.FailureResult("Selected employee not found");

                if (employee.Status != EmployeeStatus.Draft)
                    return Result<OfferLetterDto>.FailureResult("Selected employee must be in Draft status");

                // Validate that the name matches (optional, but good for consistency)
                // For now, we trust the DTO but ensure linking
            }

            var offerLetter = new OfferLetter
            {
                CandidateName = dto.CandidateName,
                CandidateEmail = dto.CandidateEmail,
                CandidatePhone = dto.CandidatePhone,
                CandidateAddress = dto.CandidateAddress,
                DepartmentId = dto.DepartmentId,
                DesignationId = dto.DesignationId,
                BasicSalary = dto.BasicSalary,
                HRA = dto.HRA,
                ConveyanceAllowance = dto.ConveyanceAllowance,
                MedicalAllowance = dto.MedicalAllowance,
                SpecialAllowance = dto.SpecialAllowance,
                OtherAllowances = dto.OtherAllowances,
                PF = dto.PF,
                ESI = dto.ESI,
                ProfessionalTax = dto.ProfessionalTax,
                TDS = dto.TDS,
                OtherDeductions = dto.OtherDeductions,
                JoiningDate = dto.JoiningDate,
                Location = dto.Location,
                AdditionalTerms = dto.AdditionalTerms,
                Status = OfferLetterStatus.Draft,
                GeneratedOn = DateTime.UtcNow,
                GeneratedBy = createdBy,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow,
                // New Fields (WP3)
                EmployeeId = dto.EmployeeId, // Link to draft employee (will be used for acceptance later)
                TemplateId = dto.TemplateId
            };

            // Generate Content from Template if provided
            if (dto.TemplateId.HasValue)
            {
                var template = await _unitOfWork.Repository<OfferLetterTemplate>().GetByIdAsync(dto.TemplateId.Value);
                if (template != null)
                {
                    // Calculate CTC (reused logic)
                    var ctc = dto.BasicSalary + dto.HRA + dto.ConveyanceAllowance + dto.MedicalAllowance + dto.SpecialAllowance + dto.OtherAllowances;
                    var gross = ctc; // Assuming no deductions from CTC for Gross calculation as per existing logic
                    
                    // Fetch Department/Designation names
                    var dept = await _unitOfWork.Repository<Department>().GetByIdAsync(dto.DepartmentId);
                    var desig = await _unitOfWork.Repository<Designation>().GetByIdAsync(dto.DesignationId);

                    var placeholderValues = GetPlaceholderValues(offerLetter, dept, desig);

                    var renderedContent = template.Content;
                    foreach (var placeholder in placeholderValues)
                    {
                        var placeholderKey = $"{{{{{placeholder.Key}}}}}";
                        renderedContent = renderedContent.Replace(placeholderKey, placeholder.Value ?? string.Empty);
                    }
                    offerLetter.GeneratedContent = renderedContent;
                }
            }

            // Calculate totals
            CalculateOfferTotals(offerLetter);

            var offerLetterRepo = _unitOfWork.Repository<OfferLetter>();
            await offerLetterRepo.AddAsync(offerLetter);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Offer letter created for {CandidateName} by {CreatedBy}",
                dto.CandidateName, createdBy);

            await LoadNavigationPropertiesAsync(offerLetter);
            var offerLetterDto = _mapper.Map<OfferLetterDto>(offerLetter);

            return Result<OfferLetterDto>.SuccessResult(offerLetterDto, "Offer letter created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating offer letter");
            return Result<OfferLetterDto>.FailureResult("Error creating offer letter");
        }
    }

    public async Task<Result<OfferLetterDto>> UpdateOfferLetterStatusAsync(int id, OfferLetterStatus status, string updatedBy)
    {
        try
        {
            var offerLetterRepo = _unitOfWork.Repository<OfferLetter>();
            var offerLetter = await offerLetterRepo.GetByIdAsync(id);

            if (offerLetter == null)
            {
                return Result<OfferLetterDto>.FailureResult("Offer letter not found");
            }

            offerLetter.Status = status;
            offerLetter.ModifiedBy = updatedBy;
            offerLetter.ModifiedOn = DateTime.UtcNow;

            offerLetterRepo.Update(offerLetter);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Offer letter {OfferLetterId} status updated to {Status} by {UpdatedBy}",
                id, status, updatedBy);

            await LoadNavigationPropertiesAsync(offerLetter);
            var offerLetterDto = _mapper.Map<OfferLetterDto>(offerLetter);

            return Result<OfferLetterDto>.SuccessResult(offerLetterDto, "Status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating offer letter status {OfferLetterId}", id);
            return Result<OfferLetterDto>.FailureResult("Error updating status");
        }
    }

    public async Task<Result> DeleteOfferLetterAsync(int id)
    {
        try
        {
            var offerLetterRepo = _unitOfWork.Repository<OfferLetter>();
            var offerLetter = await offerLetterRepo.GetByIdAsync(id);

            if (offerLetter == null)
            {
                return Result.FailureResult("Offer letter not found");
            }

            offerLetterRepo.Remove(offerLetter);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Offer letter {OfferLetterId} deleted", id);

            return Result.SuccessResult("Offer letter deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting offer letter {OfferLetterId}", id);
            return Result.FailureResult("Error deleting offer letter");
        }
    }

    public async Task<Result<byte[]>> GenerateOfferLetterPdfAsync(int id)
    {
        try
        {
            var offerLetterRepo = _unitOfWork.Repository<OfferLetter>();
            var offerLetter = await offerLetterRepo.GetByIdAsync(id);

            if (offerLetter == null)
            {
                return Result<byte[]>.FailureResult("Offer letter not found");
            }

            await LoadNavigationPropertiesAsync(offerLetter);

            using var memoryStream = new MemoryStream();

            // Use HTML Template if available
            if (!string.IsNullOrWhiteSpace(offerLetter.GeneratedContent))
            {
                // Inject custom CSS that perfectly matches the editor's visual spacing and padding
                string css = @"
                    <style>
                        @page { 
                            size: A4; 
                            margin: 0; 
                        }
                        * { box-sizing: border-box; -webkit-print-color-adjust: exact; }
                        body { 
                            font-family: Helvetica, Arial, sans-serif; 
                            font-size: 11pt; 
                            line-height: 1.5; 
                            color: #000; 
                            margin: 0; 
                            padding: 20mm; 
                            background-color: #fff;
                            word-wrap: break-word;
                        }
                        h1, h2, h3, h4, h5, h6 { 
                            margin-top: 15pt; 
                            margin-bottom: 10pt; 
                            line-height: 1.2; 
                            font-weight: bold; 
                            page-break-after: avoid; 
                        }
                        p { 
                            margin-top: 0; 
                            margin-bottom: 10pt; 
                            break-inside: auto; 
                            orphans: 1; widows: 1;
                        }
                        
                        /* Alignment Support */
                        .text-left { text-align: left; }
                        .text-center { text-align: center; }
                        .text-right { text-align: right; }
                        .text-justify { text-align: justify; }

                        ul, ol { margin-top: 0; margin-bottom: 15pt; padding-left: 25pt; }
                        li { margin-bottom: 5pt; }
                        table { width: 100%; border-collapse: collapse; margin-bottom: 20pt; table-layout: fixed; }
                        td, th { padding: 8pt; border: 1px solid #333; vertical-align: top; }
                        
                        /* Fix for Summernote specific spacing */
                        p:empty, p br { line-height: 1.5; }
                        .pdf-wrapper { width: 100%; white-space: normal; }

                        .page-break { page-break-after: always !important; }
                    </style>";

                string htmlContent = $@"<!DOCTYPE html><html><head>{css}</head><body><div class='pdf-wrapper'>{offerLetter.GeneratedContent}</div></body></html>";
                
                // Use ConverterProperties
                var props = new ConverterProperties();
                
                // Convert HTML to PDF
                HtmlConverter.ConvertToPdf(htmlContent, memoryStream, props);
            }
            else
            {
                // Fallback: Legacy manual generation (kept as safety net or for old records)
                using var writer = new PdfWriter(memoryStream);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                // Company Header
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var header = new Paragraph("WorkAxis HRMS")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(22)
                    .SetFont(boldFont);
                document.Add(header);

                document.Add(new Paragraph("NOTE: This offer letter was generated without a template.").SetFontColor(ColorConstants.RED));
                
                // ... (Simplified fallback content)
                document.Add(new Paragraph($"Date: {offerLetter.GeneratedOn:dd MMMM yyyy}"));
                document.Add(new Paragraph($"Dear {offerLetter.CandidateName},"));
                document.Add(new Paragraph($"We are pleased to offer you the position of {offerLetter.Designation.Title}."));
                document.Add(new Paragraph($"CTC: {offerLetter.CTC:N2}"));
                
                document.Close();
            }

            _logger.LogInformation("Generated offer letter PDF for {OfferLetterId}", id);
            return Result<byte[]>.SuccessResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating offer letter PDF for {OfferLetterId}", id);
            return Result<byte[]>.FailureResult($"Error generating PDF: {ex.Message}");
        }
    }

    private async Task LoadNavigationPropertiesAsync(OfferLetter offerLetter)
    {
        var departmentRepo = _unitOfWork.Repository<Department>();
        var designationRepo = _unitOfWork.Repository<Designation>();

        offerLetter.Department = (await departmentRepo.GetByIdAsync(offerLetter.DepartmentId))!;
        offerLetter.Designation = (await designationRepo.GetByIdAsync(offerLetter.DesignationId))!;
    }

    // PDF Helper Methods
    private static void AddDetailRow(Table table, string label, string value)
    {
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        table.AddCell(new Cell().Add(new Paragraph(label).SetFont(boldFont)));
        table.AddCell(new Cell().Add(new Paragraph(value)));
    }

    private static void AddSalaryHeaderRow(Table table, string col1, string col2)
    {
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        table.AddCell(new Cell().Add(new Paragraph(col1).SetFont(boldFont))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.LEFT));
        table.AddCell(new Cell().Add(new Paragraph(col2).SetFont(boldFont))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.RIGHT));
    }

    private static void AddSalaryRow(Table table, string component, decimal amount)
    {
        table.AddCell(new Cell().Add(new Paragraph(component)));
        table.AddCell(new Cell().Add(new Paragraph(amount.ToString("N2")))
            .SetTextAlignment(TextAlignment.RIGHT));
    }

    private static void AddSalaryTotalRow(Table table, string label, decimal amount)
    {
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        table.AddCell(new Cell().Add(new Paragraph(label).SetFont(boldFont))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
        table.AddCell(new Cell().Add(new Paragraph(amount.ToString("N2")).SetFont(boldFont))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.RIGHT));
    }

    public async Task<Result<IEnumerable<OfferLetterDto>>> BulkGenerateOfferLettersAsync(BulkOfferGenerationDto dto, string createdBy)
    {
        try
        {
            var generatedOffers = new List<OfferLetterDto>();
            var errors = new List<string>();

            foreach (var employeeData in dto.Employees)
            {
                try
                {
                    // Calculate CTC
                    var ctc = employeeData.BasicSalary + employeeData.HRA + employeeData.ConveyanceAllowance +
                             employeeData.MedicalAllowance + employeeData.SpecialAllowance + employeeData.OtherAllowances;
                    var offerLetter = new OfferLetter
                    {
                        CandidateName = employeeData.CandidateName,
                        CandidateEmail = employeeData.CandidateEmail,
                        CandidatePhone = employeeData.CandidatePhone ?? "",
                        CandidateAddress = employeeData.CandidateAddress ?? "",
                        DepartmentId = employeeData.DepartmentId,
                        DesignationId = employeeData.DesignationId,
                        BasicSalary = employeeData.BasicSalary,
                        HRA = employeeData.HRA,
                        ConveyanceAllowance = employeeData.ConveyanceAllowance,
                        MedicalAllowance = employeeData.MedicalAllowance,
                        SpecialAllowance = employeeData.SpecialAllowance,
                        OtherAllowances = employeeData.OtherAllowances,
                        PF = employeeData.PF,
                        ESI = employeeData.ESI,
                        ProfessionalTax = employeeData.ProfessionalTax,
                        TDS = employeeData.TDS,
                        OtherDeductions = employeeData.OtherDeductions,
                        JoiningDate = employeeData.JoiningDate,
                        Location = employeeData.Location,
                        AdditionalTerms = employeeData.AdditionalTerms,
                        Status = OfferLetterStatus.Draft,
                        GeneratedOn = DateTime.UtcNow,
                        GeneratedBy = createdBy,
                        CreatedBy = createdBy,
                        CreatedOn = DateTime.UtcNow,
                        TemplateId = dto.TemplateId
                    };

                    // Calculate totals
                    CalculateOfferTotals(offerLetter);

                    // Load navigation properties to get department and designation names
                    var department = await _unitOfWork.Repository<Department>().GetByIdAsync(employeeData.DepartmentId);
                    var designation = await _unitOfWork.Repository<Designation>().GetByIdAsync(employeeData.DesignationId);

                    // Generate content from template
                    var placeholderValues = GetPlaceholderValues(offerLetter, department, designation);

                    // Get template and render content
                    var template = await _unitOfWork.Repository<OfferLetterTemplate>().GetByIdAsync(dto.TemplateId);
                    if (template != null)
                    {
                        var renderedContent = template.Content;
                        foreach (var placeholder in placeholderValues)
                        {
                            var placeholderKey = $"{{{{{placeholder.Key}}}}}";
                            renderedContent = renderedContent.Replace(placeholderKey, placeholder.Value ?? string.Empty);
                        }
                        offerLetter.GeneratedContent = renderedContent;
                    }

                    await _unitOfWork.Repository<OfferLetter>().AddAsync(offerLetter);
                    await _unitOfWork.SaveChangesAsync();

                    await LoadNavigationPropertiesAsync(offerLetter);
                    var offerLetterDto = _mapper.Map<OfferLetterDto>(offerLetter);
                    generatedOffers.Add(offerLetterDto);

                    _logger.LogInformation("Generated offer letter for {CandidateName} by {User}", employeeData.CandidateName, createdBy);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating offer letter for {CandidateName}", employeeData.CandidateName);
                    errors.Add($"Failed to generate offer for {employeeData.CandidateName}: {ex.Message}");
                }
            }

            if (generatedOffers.Count == 0)
            {
                return Result<IEnumerable<OfferLetterDto>>.FailureResult("Failed to generate any offer letters. " + string.Join("; ", errors));
            }

            var message = generatedOffers.Count == dto.Employees.Count
                ? $"Successfully generated {generatedOffers.Count} offer letters"
                : $"Generated {generatedOffers.Count} out of {dto.Employees.Count} offer letters. Errors: {string.Join("; ", errors)}";

            return Result<IEnumerable<OfferLetterDto>>.SuccessResult(generatedOffers, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk offer letter generation");
            return Result<IEnumerable<OfferLetterDto>>.FailureResult("Error generating offer letters");
        }
    }

    public async Task<Result<EmployeeDto>> AcceptOfferAndCreateEmployeeAsync(int offerLetterId, string acceptedBy)
    {
        _logger.LogInformation("Starting offer acceptance process for offer {OfferId} by {User}", offerLetterId, acceptedBy);

        try
        {
            // 1. Get and validate offer letter
            var offerRepo = _unitOfWork.Repository<OfferLetter>();
            var offer = await offerRepo.GetByIdAsync(offerLetterId);

            if (offer == null)
            {
                _logger.LogWarning("Offer letter {OfferId} not found", offerLetterId);
                return Result<EmployeeDto>.FailureResult("Offer letter not found");
            }

            if (offer.Status != OfferLetterStatus.Sent)
            {
                _logger.LogWarning("Offer {OfferId} has invalid status {Status} for acceptance", offerLetterId, offer.Status);
                return Result<EmployeeDto>.FailureResult($"Only sent offers can be accepted. Current status: {offer.Status}");
            }

            // [Enterprise Flow] Verify Linkage
            if (offer.EmployeeId == null)
            {
                _logger.LogWarning("Offer {OfferId} is not linked to a draft employee", offerLetterId);
                return Result<EmployeeDto>.FailureResult("Offer letter must be linked to a draft employee to be accepted.");
            }

            // 2. Fetch linked Draft Employee
            var employeeRepo = _unitOfWork.Repository<Employee>();
            var employee = await employeeRepo.GetByIdAsync(offer.EmployeeId.Value);

            if (employee == null)
            {
                 return Result<EmployeeDto>.FailureResult("Linked draft employee not found.");
            }

            if (employee.Status != EmployeeStatus.Draft)
            {
                 return Result<EmployeeDto>.FailureResult($"Linked employee is not in Draft status. Current status: {employee.Status}");
            }

            // 3. Generate unique PERMANENT employee code (Replace DRAFT code)
            var existingEmployees = await employeeRepo.GetAllAsync();
            var maxCode = existingEmployees
                .Select(e => e.EmployeeCode)
                .Where(code => code.StartsWith("EMP"))
                .Select(code => int.TryParse(code.Substring(3), out var num) ? num : 0)
                .DefaultIfEmpty(0)
                .Max();
            var permanentCode = $"EMP{(maxCode + 1):D4}";

            _logger.LogInformation("Promoting Draft Employee {DraftId} to Active. New Code: {PermanentCode}", employee.Id, permanentCode);

            // 4. Update Employee Details
            employee.EmployeeCode = permanentCode;
            employee.Status = EmployeeStatus.Active;
            
            // Update fields from Offer (if changed or explicit in offer)
            var nameParts = offer.CandidateName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            employee.FirstName = nameParts.Length > 0 ? nameParts[0] : offer.CandidateName;
            employee.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";
            
            employee.DepartmentId = offer.DepartmentId;
            employee.DesignationId = offer.DesignationId;
            employee.JoiningDate = offer.JoiningDate;
            if (!string.IsNullOrEmpty(offer.CandidateAddress)) employee.Address = offer.CandidateAddress;
            if (!string.IsNullOrEmpty(offer.CandidatePhone)) employee.Phone = offer.CandidatePhone;

            // 5. Create Salary Record
            var salaryRepo = _unitOfWork.Repository<Salary>();
            
            // Deactivate any existing active salary (unlikely for Draft, but safe)
            var existingSalaries = await salaryRepo.FindAsync(s => s.EmployeeId == employee.Id && s.IsActive);
            foreach (var s in existingSalaries)
            {
                s.IsActive = false;
                s.EffectiveTo = DateTime.UtcNow;
                salaryRepo.Update(s);
            }

            var salary = new Salary
            {
                EmployeeId = employee.Id,
                BasicSalary = offer.BasicSalary,
                HRA = offer.HRA,
                ConveyanceAllowance = offer.ConveyanceAllowance,
                MedicalAllowance = offer.MedicalAllowance,
                SpecialAllowance = offer.SpecialAllowance,
                OtherAllowances = offer.OtherAllowances,
                PF = offer.PF,
                ESI = offer.ESI,
                ProfessionalTax = offer.ProfessionalTax,
                TDS = offer.TDS,
                OtherDeductions = offer.OtherDeductions,
                GrossSalary = offer.GrossSalary,
                TotalDeductions = offer.TotalDeductions,
                NetSalary = offer.NetSalary,
                CTC = offer.CTC,
                EffectiveFrom = offer.JoiningDate,
                IsActive = true,
                CreatedBy = acceptedBy,
                CreatedOn = DateTime.UtcNow
            };

            await salaryRepo.AddAsync(salary);
            
            // 6. Update Offer Status
            offer.Status = OfferLetterStatus.Accepted;
            offer.AcceptedOn = DateTime.UtcNow;
            offer.ModifiedBy = acceptedBy;
            offer.ModifiedOn = DateTime.UtcNow;
            
            offerRepo.Update(offer);
            employeeRepo.Update(employee); // Persist employee changes

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeCode} activated successfully from offer {OfferId}", permanentCode, offerLetterId);

            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return Result<EmployeeDto>.SuccessResult(employeeDto, "Offer accepted and employee activated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting offer letter {OfferId}: {ErrorMessage}", offerLetterId, ex.Message);
            return Result<EmployeeDto>.FailureResult($"Error accepting offer: {ex.Message}. Please check logs for details.");
        }
    }

    private void CalculateOfferTotals(OfferLetter offer)
    {
        // Calculate Gross Salary (all allowances)
        offer.GrossSalary = offer.BasicSalary + offer.HRA + offer.ConveyanceAllowance +
                           offer.MedicalAllowance + offer.SpecialAllowance + offer.OtherAllowances;

        // Calculate Total Deductions
        offer.TotalDeductions = offer.PF + offer.ESI + offer.ProfessionalTax +
                               offer.TDS + offer.OtherDeductions;

        // CTC = Gross + PF (Employer Contribution)
        offer.CTC = offer.GrossSalary + offer.PF;

        // Net Salary = Gross - Deductions
        offer.NetSalary = offer.GrossSalary - offer.TotalDeductions;
    }

    private Dictionary<string, string> GetPlaceholderValues(OfferLetter offer, Department? dept, Designation? desig)
    {
        var values = new Dictionary<string, string>
        {
            // Candidate Info
            { "CANDIDATE_NAME", offer.CandidateName },
            { "CANDIDATE_EMAIL", offer.CandidateEmail },
            { "CANDIDATE_PHONE", offer.CandidatePhone ?? "" },
            { "CANDIDATE_ADDRESS", offer.CandidateAddress ?? "" },
            
            // Job Info
            { "DESIGNATION", desig?.Title ?? "" },
            { "DEPARTMENT", dept?.Name ?? "" },
            { "JOINING_DATE", offer.JoiningDate.ToString("dd MMMM yyyy") },
            { "LOCATION", offer.Location ?? "" },
            { "COMPANY_NAME", "WorkAxis HRMS" },

            // Annual Salary Components
            { "BASIC_SALARY", offer.BasicSalary.ToString("N2") },
            { "HRA", offer.HRA.ToString("N2") },
            { "CONVEYANCE_ALLOWANCE", offer.ConveyanceAllowance.ToString("N2") },
            { "MEDICAL_ALLOWANCE", offer.MedicalAllowance.ToString("N2") },
            { "SPECIAL_ALLOWANCE", offer.SpecialAllowance.ToString("N2") },
            { "OTHER_ALLOWANCES", offer.OtherAllowances.ToString("N2") },
            { "GROSS_SALARY", offer.GrossSalary.ToString("N2") },
            { "PF", offer.PF.ToString("N2") },
            { "ESI", offer.ESI.ToString("N2") },
            { "PROFESSIONAL_TAX", offer.ProfessionalTax.ToString("N2") },
            { "TDS", offer.TDS.ToString("N2") },
            { "OTHER_DEDUCTIONS", offer.OtherDeductions.ToString("N2") },
            { "TOTAL_DEDUCTIONS", offer.TotalDeductions.ToString("N2") },
            { "NET_SALARY", offer.NetSalary.ToString("N2") },
            { "CTC", offer.CTC.ToString("N2") },

            // Monthly Salary Components
            { "BASIC_SALARY_MONTHLY", (offer.BasicSalary / 12).ToString("N2") },
            { "HRA_MONTHLY", (offer.HRA / 12).ToString("N2") },
            { "CONVEYANCE_MONTHLY", (offer.ConveyanceAllowance / 12).ToString("N2") },
            { "MEDICAL_MONTHLY", (offer.MedicalAllowance / 12).ToString("N2") },
            { "SPECIAL_MONTHLY", (offer.SpecialAllowance / 12).ToString("N2") },
            { "OTHER_ALLOWANCES_MONTHLY", (offer.OtherAllowances / 12).ToString("N2") },
            { "GROSS_SALARY_MONTHLY", (offer.GrossSalary / 12).ToString("N2") },
            { "PF_MONTHLY", (offer.PF / 12).ToString("N2") },
            { "ESI_MONTHLY", (offer.ESI / 12).ToString("N2") },
            { "PROFESSIONAL_TAX_MONTHLY", (offer.ProfessionalTax / 12).ToString("N2") },
            { "TDS_MONTHLY", (offer.TDS / 12).ToString("N2") },
            { "OTHER_DEDUCTIONS_MONTHLY", (offer.OtherDeductions / 12).ToString("N2") },
            { "TOTAL_DEDUCTIONS_MONTHLY", (offer.TotalDeductions / 12).ToString("N2") },
            { "NET_SALARY_MONTHLY", (offer.NetSalary / 12).ToString("N2") },
            { "CTC_MONTHLY", (offer.CTC / 12).ToString("N2") },

            // Dynamic Values
            { "TODAY_DATE", DateTime.Now.ToString("dd MMMM yyyy") },
            { "TODAY_TIME", DateTime.Now.ToString("hh:mm tt") },
            { "CURRENT_YEAR", DateTime.Now.Year.ToString() }
        };

        return values;
    }

    private async Task LoadEmployeeNavigationProperties(Employee employee)
    {
        var departmentRepo = _unitOfWork.Repository<Department>();
        var designationRepo = _unitOfWork.Repository<Designation>();

        employee.Department = await departmentRepo.GetByIdAsync(employee.DepartmentId) ?? new Department();
        employee.Designation = await designationRepo.GetByIdAsync(employee.DesignationId) ?? new Designation();
    }
}
