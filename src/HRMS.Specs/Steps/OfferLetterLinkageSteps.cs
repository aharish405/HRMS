using FluentAssertions;
using HRMS.Application.DTOs.OfferLetter;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Infrastructure.Data;
using HRMS.Shared.Models;
using HRMS.Specs.Drivers;
using HRMS.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HRMS.Specs.Steps
{
    [Binding]
    public class OfferLetterLinkageSteps : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly ScenarioContext _scenarioContext;
        private IServiceScope _scope;
        private IOfferLetterService _offerLetterService;
        private ApplicationDbContext _dbContext;

        private CreateOfferLetterDto _createDto;
        private Result<OfferLetterDto> _result;
        private int _draftEmployeeId;
        private int _activeEmployeeId;
        private int _templateId;
        private string _userName = "admin@workaxis.com";

        public OfferLetterLinkageSteps(CustomWebApplicationFactory<Program> factory, ScenarioContext scenarioContext)
        {
            _factory = factory;
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void Setup()
        {
            _scope = _factory.Services.CreateScope();
            _offerLetterService = _scope.ServiceProvider.GetRequiredService<IOfferLetterService>();
            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        [AfterScenario]
        public void Teardown()
        {
            _scope?.Dispose();
        }

        [Given(@"a draft employee exists with name ""([^""]*)"" and email ""([^""]*)""")]
        public async Task GivenADraftEmployeeExists(string name, string email)
        {
            var parts = name.Split(' ');
            var firstName = parts[0];
            var lastName = parts.Length > 1 ? parts[1] : "";

            // Check if exists
            var existing = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (existing != null)
            {
                _draftEmployeeId = existing.Id;
                return;
            }

            var employee = new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                EmployeeCode = $"DRAFT_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Status = EmployeeStatus.Draft,
                DepartmentId = 1,
                DesignationId = 1,
                CreatedBy = _userName,
                CreatedOn = DateTime.UtcNow
            };

            // Ensure Dep/Desig exist
            if (!await _dbContext.Departments.AnyAsync(d => d.Id == 1))
            {
                _dbContext.Departments.Add(new Department { Id = 1, Name = "IT", Code = "IT" });
            }
            if (!await _dbContext.Designations.AnyAsync(d => d.Id == 1))
            {
                _dbContext.Designations.Add(new Designation { Id = 1, Title = "Software Engineer", Level = 1 });
            }

            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();
            _draftEmployeeId = employee.Id;
        }

        [Given(@"a valid offer letter template ""([^""]*)""")]
        public async Task GivenAValidOfferLetterTemplate(string templateName)
        {
            var existing = await _dbContext.OfferLetterTemplates.FirstOrDefaultAsync(t => t.Name == templateName);
            if (existing != null)
            {
                _templateId = existing.Id;
                return;
            }

            var template = new OfferLetterTemplate
            {
                Name = templateName,
                Content = "<p>Welcome {{CandidateName}}</p>",
                IsActive = true,
                CreatedBy = _userName,
                CreatedOn = DateTime.UtcNow
            };
            _dbContext.OfferLetterTemplates.Add(template);
            await _dbContext.SaveChangesAsync();
            _templateId = template.Id;
        }

        [When(@"I create an offer letter linked to ""([^""]*)"" using template ""([^""]*)""")]
        public async Task WhenICreateAnOfferLetterLinkedToUsingTemplate(string candidateName, string templateName)
        {
             // Retrieve employee details to populate the DTO (simulating UI behavior)
            var employee = await _dbContext.Employees.FindAsync(_draftEmployeeId);

            _createDto = new CreateOfferLetterDto
            {
                // New Fields needed in DTO
                EmployeeId = _draftEmployeeId, 
                TemplateId = _templateId,
                
                // Existing fields
                CandidateName = candidateName,
                CandidateEmail = employee.Email,
                CandidatePhone = "1234567890",
                CandidateAddress = "123 Test St",
                DepartmentId = 1,
                DesignationId = 1,
                BasicSalary = 50000,
                HRA = 20000,
                ConveyanceAllowance = 2000,
                MedicalAllowance = 1250,
                SpecialAllowance = 10000,
                JoiningDate = DateTime.Today.AddDays(15),
                Location = "Remote"
            };
            
            _result = await _offerLetterService.CreateOfferLetterAsync(_createDto, _userName);
        }

        [Then(@"the offer letter should be created successfully")]
        public void ThenTheOfferLetterShouldBeCreatedSuccessfully()
        {
            _result.Should().NotBeNull();
            _result.Success.Should().BeTrue($"because offer creation failed: {_result.Message}");
            _result.Data.Id.Should().BeGreaterThan(0);
        }

        [Then(@"the candidate name should be ""([^""]*)""")]
        public void ThenTheCandidateNameShouldBe(string expectedName)
        {
            _result.Data.CandidateName.Should().Be(expectedName);
        }

        [Then(@"the offer should be linked to the employee")]
        public async Task ThenTheOfferShouldBeLinkedToTheEmployee()
        {
            var offer = await _dbContext.OfferLetters.FindAsync(_result.Data.Id);
            offer.EmployeeId.Should().Be(_draftEmployeeId);
        }

        [Then(@"the offer status should be ""([^""]*)""")]
        public void ThenTheOfferStatusShouldBe(string status)
        {
            _result.Data.Status.ToString().Should().Be(status);
        }

        [Given(@"an active employee exists with name ""([^""]*)""")]
        public async Task GivenAnActiveEmployeeExists(string name)
        {
            // Create active employee
             var employee = new Employee
            {
                FirstName = name.Split(' ')[0],
                LastName = name.Contains(' ') ? name.Split(' ')[1] : "",
                Email = "active@test.com",
                EmployeeCode = $"EMP_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Status = EmployeeStatus.Active,
                DepartmentId = 1,
                DesignationId = 1,
                CreatedBy = _userName,
                CreatedOn = DateTime.UtcNow
            };
            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();
            _activeEmployeeId = employee.Id;
        }

        [When(@"I try to create an offer letter linked to ""([^""]*)""")]
        public async Task WhenITryToCreateAnOfferLetterLinkedTo(string name)
        {
             _createDto = new CreateOfferLetterDto
            {
                EmployeeId = _activeEmployeeId,
                CandidateName = name,
                CandidateEmail = "active@test.com",
                CandidatePhone = "1234567890",
                CandidateAddress = "123 Test St",
                DepartmentId = 1,
                DesignationId = 1,
                BasicSalary = 50000,
                JoiningDate = DateTime.Today.AddDays(15),
                Location = "Remote"
            };

            _result = await _offerLetterService.CreateOfferLetterAsync(_createDto, _userName);
        }
        
        [Then(@"the creation should fail with error ""([^""]*)""")]
        public void ThenTheCreationShouldFailWithError(string error)
        {
            _result.Success.Should().BeFalse();
            _result.Message.Should().Contain(error);
        }
    }
}
