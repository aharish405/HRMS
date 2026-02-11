using HRMS.Application.DTOs.OfferLetterTemplate;
using HRMS.Shared.Models;

namespace HRMS.Application.Interfaces;

public interface IOfferLetterTemplateService
{
    // Template CRUD Operations
    Task<Result<IEnumerable<OfferLetterTemplateDto>>> GetAllTemplatesAsync();
    Task<Result<OfferLetterTemplateDto>> GetTemplateByIdAsync(int id);
    Task<Result<OfferLetterTemplateDto>> GetDefaultTemplateAsync();
    Task<Result<OfferLetterTemplateDto>> CreateTemplateAsync(CreateOfferLetterTemplateDto dto, string createdBy);
    Task<Result<OfferLetterTemplateDto>> UpdateTemplateAsync(UpdateOfferLetterTemplateDto dto, string modifiedBy);
    Task<Result<bool>> DeleteTemplateAsync(int id);
    Task<Result<bool>> SetDefaultTemplateAsync(int id);

    // Template Operations
    Task<Result<OfferLetterTemplateDto>> CloneTemplateAsync(int id, string clonedBy);
    Task<Result<string>> PreviewTemplateAsync(int templateId, Dictionary<string, string> placeholderValues);
    Task<Result<string>> RenderTemplateAsync(int templateId, Dictionary<string, string> placeholderValues);

    // Placeholder Operations
    Task<Result<IEnumerable<TemplatePlaceholderDto>>> GetAllPlaceholdersAsync();
    Task<Result<IEnumerable<TemplatePlaceholderDto>>> GetPlaceholdersByCategoryAsync(int category);
}
