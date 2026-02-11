using FluentAssertions;
using HRMS.Application.DTOs.OfferLetterTemplate;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using HRMS.Shared.Models;
using HRMS.Specs.Drivers;
using HRMS.Web;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HRMS.Specs.Steps
{
    [Binding]
    public class OfferLetterTemplateSteps : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly ScenarioContext _scenarioContext;
        private IServiceScope _scope;
        private IOfferLetterTemplateService _templateService;
        private ApplicationDbContext _dbContext;

        private CreateOfferLetterTemplateDto _createDto;
        private UpdateOfferLetterTemplateDto _updateDto;
        private Result<OfferLetterTemplateDto> _result;
        private string _userName = "admin@workaxis.com";

        public OfferLetterTemplateSteps(CustomWebApplicationFactory<Program> factory, ScenarioContext scenarioContext)
        {
            _factory = factory;
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void Setup()
        {
            _scope = _factory.Services.CreateScope();
            _templateService = _scope.ServiceProvider.GetRequiredService<IOfferLetterTemplateService>();
            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        [AfterScenario]
        public void Teardown()
        {
            _scope?.Dispose();
        }

        [Given(@"I have a template name ""(.*)""")]
        public void GivenIHaveATemplateName(string name)
        {
            _createDto = new CreateOfferLetterTemplateDto
            {
                Name = name,
                Category = "Standard",
                IsActive = true,
                IsDefault = false
            };
        }

        [Given(@"I provide rich text content ""(.*)""")]
        public void GivenIProvideRichTextContent(string content)
        {
            _createDto.Content = content;
        }

        [When(@"I submit the create template form")]
        public async Task WhenISubmitTheCreateTemplateForm()
        {
            _result = await _templateService.CreateTemplateAsync(_createDto, _userName);
        }

        [Then(@"the template should be saved successfully")]
        public void ThenTheTemplateShouldBeSavedSuccessfully()
        {
            _result.Success.Should().BeTrue($"because template creation should succeed but failed with: {_result.Message}");
            _result.Data.Should().NotBeNull();
            _result.Data.Id.Should().BeGreaterThan(0);
        }

        [Then(@"the template name should be ""(.*)""")]
        public void ThenTheTemplateNameShouldBe(string expectedName)
        {
            _result.Data.Name.Should().Be(expectedName);
        }

        [Given(@"a template ""(.*)"" already exists")]
        public async Task GivenATemplateAlreadyExists(string name)
        {
            // Check if it exists, if not create it
            if (!_dbContext.OfferLetterTemplates.Any(t => t.Name == name))
            {
                var dto = new CreateOfferLetterTemplateDto
                {
                    Name = name,
                    Content = "<p>Existing content</p>",
                    Category = "Standard",
                    IsActive = true
                };
                await _templateService.CreateTemplateAsync(dto, _userName);
            }
        }

        [When(@"I try to create another template with name ""(.*)""")]
        public async Task WhenITryToCreateAnotherTemplateWithName(string name)
        {
            _createDto = new CreateOfferLetterTemplateDto
            {
                Name = name,
                Content = "<p>Duplicate content</p>",
                Category = "Standard",
                IsActive = true
            };
            _result = await _templateService.CreateTemplateAsync(_createDto, _userName);
        }

        [Then(@"the creation should fail with an error message ""(.*)""")]
        public void ThenTheCreationShouldFailWithAnErrorMessage(string expectedMessage)
        {
            _result.Success.Should().BeFalse();
            _result.Message.Should().Contain(expectedMessage);
        }

        [Given(@"an existing template ""(.*)""")]
        public async Task GivenAnExistingTemplate(string name)
        {
            await GivenATemplateAlreadyExists(name);
            var template = _dbContext.OfferLetterTemplates.FirstOrDefault(t => t.Name == name);
            
            _updateDto = new UpdateOfferLetterTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Content = template.Content,
                Category = template.Category,
                IsActive = template.IsActive,
                IsDefault = template.IsDefault
            };
        }

        [When(@"I update the template content to ""(.*)""")]
        public async Task WhenIUpdateTheTemplateContentTo(string newContent)
        {
            _updateDto.Content = newContent;
            _result = await _templateService.UpdateTemplateAsync(_updateDto, _userName);
        }

        [Then(@"the template should be updated successfully")]
        public void ThenTheTemplateShouldBeUpdatedSuccessfully()
        {
            _result.Success.Should().BeTrue($"because update should succeed but failed with: {_result.Message}");
        }

        [Then(@"the content should become ""(.*)""")]
        public void ThenTheContentShouldBecome(string expectedContent)
        {
            _result.Data.Content.Should().Be(expectedContent);
        }
    }
}
