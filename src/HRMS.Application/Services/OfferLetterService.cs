using AutoMapper;
using HRMS.Application.DTOs.OfferLetter;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Models;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
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
            // Calculate CTC
            var ctc = dto.BasicSalary + dto.HRA + dto.ConveyanceAllowance +
                     dto.MedicalAllowance + dto.SpecialAllowance + dto.OtherAllowances;

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
                CTC = ctc,
                JoiningDate = dto.JoiningDate,
                Location = dto.Location,
                AdditionalTerms = dto.AdditionalTerms,
                Status = OfferLetterStatus.Draft,
                GeneratedOn = DateTime.UtcNow,
                GeneratedBy = createdBy,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow
            };

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
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Company Header
            var header = new Paragraph("WorkAxis HRMS")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(22)
                .SetBold();
            document.Add(header);

            var companyAddress = new Paragraph("123 Business Park, Tech City, TC 12345\nEmail: hr@workaxis.com | Phone: +1-234-567-8900")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10);
            document.Add(companyAddress);

            document.Add(new Paragraph("\n"));

            // Date
            var date = new Paragraph($"Date: {offerLetter.GeneratedOn:dd MMMM yyyy}")
                .SetFontSize(11);
            document.Add(date);

            document.Add(new Paragraph("\n"));

            // Candidate Address
            var candidateAddress = new Paragraph($"{offerLetter.CandidateName}\n{offerLetter.CandidateAddress}")
                .SetFontSize(11);
            document.Add(candidateAddress);

            document.Add(new Paragraph("\n"));

            // Subject
            var subject = new Paragraph($"Subject: Offer of Employment - {offerLetter.Designation.Title}")
                .SetFontSize(12)
                .SetBold();
            document.Add(subject);

            document.Add(new Paragraph("\n"));

            // Salutation
            var salutation = new Paragraph($"Dear {offerLetter.CandidateName},")
                .SetFontSize(11);
            document.Add(salutation);

            document.Add(new Paragraph("\n"));

            // Body
            var body1 = new Paragraph($"We are pleased to offer you the position of {offerLetter.Designation.Title} in the {offerLetter.Department.Name} department at WorkAxis HRMS. We believe that your skills and experience will be a valuable asset to our team.")
                .SetFontSize(11);
            document.Add(body1);

            document.Add(new Paragraph("\n"));

            // Position Details
            var positionHeader = new Paragraph("Position Details:")
                .SetFontSize(12)
                .SetBold();
            document.Add(positionHeader);

            var positionTable = new Table(2);
            positionTable.SetWidth(UnitValue.CreatePercentValue(100));

            AddDetailRow(positionTable, "Position:", offerLetter.Designation.Title);
            AddDetailRow(positionTable, "Department:", offerLetter.Department.Name);
            AddDetailRow(positionTable, "Location:", offerLetter.Location);
            AddDetailRow(positionTable, "Joining Date:", offerLetter.JoiningDate.ToString("dd MMMM yyyy"));

            document.Add(positionTable);
            document.Add(new Paragraph("\n"));

            // Compensation
            var compensationHeader = new Paragraph("Compensation Details:")
                .SetFontSize(12)
                .SetBold();
            document.Add(compensationHeader);

            var salaryTable = new Table(new float[] { 3, 2 });
            salaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            AddSalaryHeaderRow(salaryTable, "Component", "Amount (₹)");
            AddSalaryRow(salaryTable, "Basic Salary", offerLetter.BasicSalary);
            AddSalaryRow(salaryTable, "House Rent Allowance (HRA)", offerLetter.HRA);
            AddSalaryRow(salaryTable, "Conveyance Allowance", offerLetter.ConveyanceAllowance);
            AddSalaryRow(salaryTable, "Medical Allowance", offerLetter.MedicalAllowance);
            AddSalaryRow(salaryTable, "Special Allowance", offerLetter.SpecialAllowance);
            if (offerLetter.OtherAllowances > 0)
            {
                AddSalaryRow(salaryTable, "Other Allowances", offerLetter.OtherAllowances);
            }
            AddSalaryTotalRow(salaryTable, "Total Annual CTC", offerLetter.CTC);

            document.Add(salaryTable);
            document.Add(new Paragraph("\n"));

            // Terms and Conditions
            var termsHeader = new Paragraph("Terms and Conditions:")
                .SetFontSize(12)
                .SetBold();
            document.Add(termsHeader);

            var terms = new List()
                .SetSymbolIndent(12)
                .SetListSymbol("\u2022");

            terms.Add(new ListItem("This offer is contingent upon successful completion of background verification."));
            terms.Add(new ListItem("You will be required to serve a probation period of 3 months."));
            terms.Add(new ListItem("Your employment will be governed by the company's policies and procedures."));
            terms.Add(new ListItem("Either party may terminate the employment with 30 days' notice."));

            if (!string.IsNullOrWhiteSpace(offerLetter.AdditionalTerms))
            {
                terms.Add(new ListItem(offerLetter.AdditionalTerms));
            }

            document.Add(terms);
            document.Add(new Paragraph("\n"));

            // Acceptance Instructions
            var acceptance = new Paragraph("Please sign and return a copy of this letter to indicate your acceptance of this offer by " +
                $"{offerLetter.JoiningDate.AddDays(-7):dd MMMM yyyy}.")
                .SetFontSize(11);
            document.Add(acceptance);

            document.Add(new Paragraph("\n"));

            // Closing
            var closing = new Paragraph("We look forward to welcoming you to our team!")
                .SetFontSize(11);
            document.Add(closing);

            document.Add(new Paragraph("\n\n"));

            var signature = new Paragraph("Sincerely,\n\nHR Department\nWorkAxis HRMS")
                .SetFontSize(11);
            document.Add(signature);

            document.Add(new Paragraph("\n\n"));

            // Acceptance Section
            var acceptanceSection = new Paragraph("Acceptance:")
                .SetFontSize(12)
                .SetBold();
            document.Add(acceptanceSection);

            var acceptanceText = new Paragraph($"I, {offerLetter.CandidateName}, accept the above offer of employment.\n\n" +
                "Signature: _____________________     Date: _____________________")
                .SetFontSize(11);
            document.Add(acceptanceText);

            document.Close();

            _logger.LogInformation("Generated offer letter PDF for {OfferLetterId}", id);

            return Result<byte[]>.SuccessResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating offer letter PDF for {OfferLetterId}", id);
            return Result<byte[]>.FailureResult("Error generating PDF");
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
        table.AddCell(new Cell().Add(new Paragraph(label)).SetBold());
        table.AddCell(new Cell().Add(new Paragraph(value)));
    }

    private static void AddSalaryHeaderRow(Table table, string col1, string col2)
    {
        table.AddCell(new Cell().Add(new Paragraph(col1))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetBold()
            .SetTextAlignment(TextAlignment.LEFT));
        table.AddCell(new Cell().Add(new Paragraph(col2))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetBold()
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
        table.AddCell(new Cell().Add(new Paragraph(label))
            .SetBold()
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
        table.AddCell(new Cell().Add(new Paragraph(amount.ToString("N2")))
            .SetBold()
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.RIGHT));
    }
}
