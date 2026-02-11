using HRMS.Application.DTOs.OfferLetterTemplate;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Models;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services;

public class OfferLetterTemplateService : IOfferLetterTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OfferLetterTemplateService> _logger;

    public OfferLetterTemplateService(
        IUnitOfWork unitOfWork,
        ILogger<OfferLetterTemplateService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<OfferLetterTemplateDto>>> GetAllTemplatesAsync()
    {
        try
        {
            var templates = await _unitOfWork.Repository<OfferLetterTemplate>()
                .GetAllAsync();

            var templateDtos = templates.Select(MapToDto).ToList();

            return Result<IEnumerable<OfferLetterTemplateDto>>.SuccessResult(templateDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving offer letter templates");
            return Result<IEnumerable<OfferLetterTemplateDto>>.FailureResult("Error retrieving templates");
        }
    }

    public async Task<Result<OfferLetterTemplateDto>> GetTemplateByIdAsync(int id)
    {
        try
        {
            var template = await _unitOfWork.Repository<OfferLetterTemplate>()
                .GetByIdAsync(id);

            if (template == null)
                return Result<OfferLetterTemplateDto>.FailureResult("Template not found");

            return Result<OfferLetterTemplateDto>.SuccessResult(MapToDto(template));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template {TemplateId}", id);
            return Result<OfferLetterTemplateDto>.FailureResult("Error retrieving template");
        }
    }

    public async Task<Result<OfferLetterTemplateDto>> GetDefaultTemplateAsync()
    {
        try
        {
            var templates = await _unitOfWork.Repository<OfferLetterTemplate>()
                .FindAsync(t => t.IsDefault && t.IsActive);

            var defaultTemplate = templates.FirstOrDefault();

            if (defaultTemplate == null)
                return Result<OfferLetterTemplateDto>.FailureResult("No default template found");

            return Result<OfferLetterTemplateDto>.SuccessResult(MapToDto(defaultTemplate));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default template");
            return Result<OfferLetterTemplateDto>.FailureResult("Error retrieving default template");
        }
    }

    public async Task<Result<OfferLetterTemplateDto>> CreateTemplateAsync(CreateOfferLetterTemplateDto dto, string createdBy)
    {
        try
        {
            // Check if template with same name already exists
            var existingTemplate = await _unitOfWork.Repository<OfferLetterTemplate>()
                .FindAsync(t => t.Name == dto.Name && !t.IsDeleted);
            
            if (existingTemplate.Any())
            {
                return Result<OfferLetterTemplateDto>.FailureResult("Template name already exists");
            }

            // If this is set as default, unset other defaults
            if (dto.IsDefault)
            {
                await UnsetAllDefaultTemplatesAsync();
            }

            var template = new OfferLetterTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                Content = dto.Content,
                Category = dto.Category,
                IsActive = dto.IsActive,
                IsDefault = dto.IsDefault,
                Version = 1,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.Repository<OfferLetterTemplate>().AddAsync(template);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created offer letter template {TemplateName} by {User}", template.Name, createdBy);

            return Result<OfferLetterTemplateDto>.SuccessResult(MapToDto(template), "Template created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return Result<OfferLetterTemplateDto>.FailureResult("Error creating template");
        }
    }

    public async Task<Result<OfferLetterTemplateDto>> UpdateTemplateAsync(UpdateOfferLetterTemplateDto dto, string modifiedBy)
    {
        try
        {
            var template = await _unitOfWork.Repository<OfferLetterTemplate>()
                .GetByIdAsync(dto.Id);

            if (template == null)
                return Result<OfferLetterTemplateDto>.FailureResult("Template not found");

            // If this is set as default, unset other defaults
            if (dto.IsDefault && !template.IsDefault)
            {
                await UnsetAllDefaultTemplatesAsync();
            }

            template.Name = dto.Name;
            template.Description = dto.Description;
            template.Content = dto.Content;
            template.Category = dto.Category;
            template.IsActive = dto.IsActive;
            template.IsDefault = dto.IsDefault;
            template.Version++;
            template.ModifiedBy = modifiedBy;
            template.ModifiedOn = DateTime.UtcNow;

            _unitOfWork.Repository<OfferLetterTemplate>().Update(template);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated offer letter template {TemplateId} by {User}", template.Id, modifiedBy);

            return Result<OfferLetterTemplateDto>.SuccessResult(MapToDto(template), "Template updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId}", dto.Id);
            return Result<OfferLetterTemplateDto>.FailureResult("Error updating template");
        }
    }

    public async Task<Result<bool>> DeleteTemplateAsync(int id)
    {
        try
        {
            var template = await _unitOfWork.Repository<OfferLetterTemplate>()
                .GetByIdAsync(id);

            if (template == null)
                return Result<bool>.FailureResult("Template not found");

            if (template.IsDefault)
                return Result<bool>.FailureResult("Cannot delete the default template");

            template.IsDeleted = true;
            _unitOfWork.Repository<OfferLetterTemplate>().Update(template);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted offer letter template {TemplateId}", id);

            return Result<bool>.SuccessResult(true, "Template deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId}", id);
            return Result<bool>.FailureResult("Error deleting template");
        }
    }

    public async Task<Result<bool>> SetDefaultTemplateAsync(int id)
    {
        try
        {
            var template = await _unitOfWork.Repository<OfferLetterTemplate>()
                .GetByIdAsync(id);

            if (template == null)
                return Result<bool>.FailureResult("Template not found");

            await UnsetAllDefaultTemplatesAsync();

            template.IsDefault = true;
            template.ModifiedOn = DateTime.UtcNow;

            _unitOfWork.Repository<OfferLetterTemplate>().Update(template);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Set template {TemplateId} as default", id);

            return Result<bool>.SuccessResult(true, "Default template set successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default template {TemplateId}", id);
            return Result<bool>.FailureResult("Error setting default template");
        }
    }

    public async Task<Result<OfferLetterTemplateDto>> CloneTemplateAsync(int id, string clonedBy)
    {
        try
        {
            var originalTemplate = await _unitOfWork.Repository<OfferLetterTemplate>()
                .GetByIdAsync(id);

            if (originalTemplate == null)
                return Result<OfferLetterTemplateDto>.FailureResult("Template not found");

            var clonedTemplate = new OfferLetterTemplate
            {
                Name = $"{originalTemplate.Name} (Copy)",
                Description = originalTemplate.Description,
                Content = originalTemplate.Content,
                Category = originalTemplate.Category,
                IsActive = true,
                IsDefault = false,
                Version = 1,
                CreatedBy = clonedBy,
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.Repository<OfferLetterTemplate>().AddAsync(clonedTemplate);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Cloned template {TemplateId} to {NewTemplateId} by {User}", id, clonedTemplate.Id, clonedBy);

            return Result<OfferLetterTemplateDto>.SuccessResult(MapToDto(clonedTemplate), "Template cloned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning template {TemplateId}", id);
            return Result<OfferLetterTemplateDto>.FailureResult("Error cloning template");
        }
    }

    public async Task<Result<string>> PreviewTemplateAsync(int templateId, Dictionary<string, string> placeholderValues)
    {
        return await RenderTemplateAsync(templateId, placeholderValues);
    }

    public async Task<Result<string>> RenderTemplateAsync(int templateId, Dictionary<string, string> placeholderValues)
    {
        try
        {
            var template = await _unitOfWork.Repository<OfferLetterTemplate>()
                .GetByIdAsync(templateId);

            if (template == null)
                return Result<string>.FailureResult("Template not found");

            var renderedContent = template.Content;

            // Replace all placeholders with actual values
            foreach (var placeholder in placeholderValues)
            {
                var placeholderKey = $"{{{{{placeholder.Key}}}}}";
                renderedContent = renderedContent.Replace(placeholderKey, placeholder.Value ?? string.Empty);
            }

            return Result<string>.SuccessResult(renderedContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {TemplateId}", templateId);
            return Result<string>.FailureResult("Error rendering template");
        }
    }

    public async Task<Result<IEnumerable<TemplatePlaceholderDto>>> GetAllPlaceholdersAsync()
    {
        try
        {
            var placeholders = await _unitOfWork.Repository<TemplatePlaceholder>()
                .FindAsync(p => p.IsActive);

            var placeholderDtos = placeholders.Select(p => new TemplatePlaceholderDto
            {
                Id = p.Id,
                PlaceholderKey = p.PlaceholderKey,
                DisplayName = p.DisplayName,
                Category = p.Category,
                DataType = p.DataType,
                Description = p.Description,
                SampleValue = p.SampleValue,
                IsRequired = p.IsRequired,
                FormatString = p.FormatString,
                IsActive = p.IsActive
            }).ToList();

            // Add Dynamic System Placeholders
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "TODAY_DATE", DisplayName = "Today's Date", Category = PlaceholderCategory.Custom, Description = "Current date", SampleValue = DateTime.Now.ToString("dd MMMM yyyy") });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "TODAY_TIME", DisplayName = "Today's Time", Category = PlaceholderCategory.Custom, Description = "Current time", SampleValue = DateTime.Now.ToString("hh:mm tt") });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "CURRENT_YEAR", DisplayName = "Current Year", Category = PlaceholderCategory.Custom, Description = "Current year", SampleValue = DateTime.Now.Year.ToString() });

            // Add Monthly Salary Placeholders
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "BASIC_SALARY_MONTHLY", DisplayName = "Basic Salary (Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly basic", SampleValue = "4,166.67" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "HRA_MONTHLY", DisplayName = "HRA (Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly HRA", SampleValue = "2,083.33" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "CONVEYANCE_MONTHLY", DisplayName = "Conveyance (Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly conveyance", SampleValue = "133.33" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "MEDICAL_MONTHLY", DisplayName = "Medical (Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly medical", SampleValue = "104.17" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "SPECIAL_MONTHLY", DisplayName = "Special Allowance (Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly special", SampleValue = "1,250.00" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "OTHER_ALLOWANCES_MONTHLY", DisplayName = "Other Allowances (Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly other allowance", SampleValue = "416.67" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "GROSS_SALARY_MONTHLY", DisplayName = "Gross Salary (Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly gross", SampleValue = "8,154.17" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "PF_MONTHLY", DisplayName = "PF (Employee - Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly PF deduction", SampleValue = "1,800.00" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "ESI_MONTHLY", DisplayName = "ESI (Employee - Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly ESI deduction", SampleValue = "150.00" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "PROFESSIONAL_TAX_MONTHLY", DisplayName = "PT (Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly PT deduction", SampleValue = "200.00" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "NET_SALARY_MONTHLY", DisplayName = "Net Salary (Monthly / In-hand)", Category = PlaceholderCategory.Salary, Description = "Monthly take-home", SampleValue = "6,000.00" });
            placeholderDtos.Add(new TemplatePlaceholderDto { PlaceholderKey = "CTC_MONTHLY", DisplayName = "CTC (Monthly)", Category = PlaceholderCategory.Salary, Description = "Monthly total cost", SampleValue = "10,000.00" });

            return Result<IEnumerable<TemplatePlaceholderDto>>.SuccessResult(placeholderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving placeholders");
            return Result<IEnumerable<TemplatePlaceholderDto>>.FailureResult("Error retrieving placeholders");
        }
    }

    public async Task<Result<IEnumerable<TemplatePlaceholderDto>>> GetPlaceholdersByCategoryAsync(int category)
    {
        try
        {
            var placeholders = await _unitOfWork.Repository<TemplatePlaceholder>()
                .FindAsync(p => p.IsActive && (int)p.Category == category);

            var placeholderDtos = placeholders.Select(p => new TemplatePlaceholderDto
            {
                Id = p.Id,
                PlaceholderKey = p.PlaceholderKey,
                DisplayName = p.DisplayName,
                Category = p.Category,
                DataType = p.DataType,
                Description = p.Description,
                SampleValue = p.SampleValue,
                IsRequired = p.IsRequired,
                FormatString = p.FormatString,
                IsActive = p.IsActive
            }).ToList();

            return Result<IEnumerable<TemplatePlaceholderDto>>.SuccessResult(placeholderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving placeholders for category {Category}", category);
            return Result<IEnumerable<TemplatePlaceholderDto>>.FailureResult("Error retrieving placeholders");
        }
    }

    // Helper Methods
    private async Task UnsetAllDefaultTemplatesAsync()
    {
        var defaultTemplates = await _unitOfWork.Repository<OfferLetterTemplate>()
            .FindAsync(t => t.IsDefault);

        foreach (var template in defaultTemplates)
        {
            template.IsDefault = false;
            _unitOfWork.Repository<OfferLetterTemplate>().Update(template);
        }
    }

    private static OfferLetterTemplateDto MapToDto(OfferLetterTemplate template)
    {
        return new OfferLetterTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Content = template.Content,
            Category = template.Category,
            IsActive = template.IsActive,
            IsDefault = template.IsDefault,
            Version = template.Version,
            CreatedOn = template.CreatedOn,
            CreatedBy = template.CreatedBy,
            ModifiedOn = template.ModifiedOn,
            ModifiedBy = template.ModifiedBy
        };
    }
}
