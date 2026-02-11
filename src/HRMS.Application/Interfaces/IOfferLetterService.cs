using HRMS.Application.DTOs.Employee;
using HRMS.Application.DTOs.OfferLetter;
using HRMS.Shared.Models;

namespace HRMS.Application.Interfaces;

public interface IOfferLetterService
{
    Task<Result<IEnumerable<OfferLetterDto>>> GetAllOfferLettersAsync();
    Task<Result<OfferLetterDto>> GetOfferLetterByIdAsync(int id);
    Task<Result<OfferLetterDto>> CreateOfferLetterAsync(CreateOfferLetterDto dto, string createdBy);
    Task<Result<OfferLetterDto>> UpdateOfferLetterStatusAsync(int id, Domain.Enums.OfferLetterStatus status, string updatedBy);
    Task<Result> DeleteOfferLetterAsync(int id);
    Task<Result<byte[]>> GenerateOfferLetterPdfAsync(int id);
    Task<Result<IEnumerable<OfferLetterDto>>> BulkGenerateOfferLettersAsync(BulkOfferGenerationDto dto, string createdBy);
    Task<Result<EmployeeDto>> AcceptOfferAndCreateEmployeeAsync(int offerLetterId, string acceptedBy);
}
