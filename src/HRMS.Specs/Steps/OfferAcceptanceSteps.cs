using FluentAssertions;
using HRMS.Application.DTOs.Employee;
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
    public class OfferAcceptanceSteps : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly ScenarioContext _scenarioContext;
        private IServiceScope _scope;
        private IOfferLetterService _offerLetterService;
        private ApplicationDbContext _dbContext;

        private int _draftEmployeeId;
        private int _offerLetterId;
        private Result<EmployeeDto> _result;
        private string _userName = "admin@workaxis.com";

        public OfferAcceptanceSteps(CustomWebApplicationFactory<Program> factory, ScenarioContext scenarioContext)
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

        [Given(@"a draft employee exists with name ""([^""]*)""")]
        public async Task GivenADraftEmployeeExists(string name)
        {
             var employee = new Employee
            {
                FirstName = name.Split(' ')[0],
                LastName = name.Contains(' ') ? name.Split(' ')[1] : "",
                Email = $"draft_{Guid.NewGuid()}@test.com",
                EmployeeCode = $"DRAFT_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Status = EmployeeStatus.Draft,
                DepartmentId = 1,
                DesignationId = 1,
                CreatedBy = _userName,
                CreatedOn = DateTime.UtcNow
            };
            
            // Ensure Dep/Desig
             if (!await _dbContext.Departments.AnyAsync(d => d.Id == 1))
            {
                _dbContext.Departments.Add(new Department { Id = 1, Name = "IT", Code = "IT" });
            }
            if (!await _dbContext.Designations.AnyAsync(d => d.Id == 1))
            {
                _dbContext.Designations.Add(new Designation { Id = 1, Title = "SE", Level = 1 });
            }

            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();
            _draftEmployeeId = employee.Id;
        }

        [Given(@"an offer letter exists for ""([^""]*)"" linked to the draft employee")]
        public async Task GivenAnOfferLetterExistsLinkedToTheDraftEmployee(string name)
        {
             var offer = new OfferLetter
            {
                EmployeeId = _draftEmployeeId,
                CandidateName = name,
                CandidateEmail = "test@test.com",
                Status = OfferLetterStatus.Draft, // Default
                BasicSalary = 50000,
                HRA = 20000,
                GeneratedOn = DateTime.UtcNow,
                CreatedBy = _userName
            };
            _dbContext.OfferLetters.Add(offer);
            await _dbContext.SaveChangesAsync();
            _offerLetterId = offer.Id;
        }

        [Given(@"the offer letter status is ""([^""]*)""")]
        public async Task GivenTheOfferLetterStatusIs(string status)
        {
            var offer = await _dbContext.OfferLetters.FindAsync(_offerLetterId);
            if (Enum.TryParse<OfferLetterStatus>(status, out var statusEnum))
            {
                offer.Status = statusEnum;
                await _dbContext.SaveChangesAsync();
            }
        }
        
        [Given(@"an offer letter exists for ""([^""]*)""")]
        public async Task GivenAnOfferLetterExistsFor(string name)
        {
             var offer = new OfferLetter
            {
                CandidateName = name,
                CandidateEmail = "test2@test.com",
                Status = OfferLetterStatus.Draft, 
                BasicSalary = 50000,
                GeneratedOn = DateTime.UtcNow,
                CreatedBy = _userName
            };
            _dbContext.OfferLetters.Add(offer);
            await _dbContext.SaveChangesAsync();
            _offerLetterId = offer.Id;
        }

        [When(@"I accept the offer letter")]
        public async Task WhenIAcceptTheOfferLetter()
        {
            _result = await _offerLetterService.AcceptOfferAndCreateEmployeeAsync(_offerLetterId, _userName);
        }
        
        [When(@"I try to accept the offer letter")]
        public async Task WhenITryToAcceptTheOfferLetter()
        {
             _result = await _offerLetterService.AcceptOfferAndCreateEmployeeAsync(_offerLetterId, _userName);
        }

        [Then(@"the employee status should be ""(.*)""")]
        public async Task ThenTheEmployeeStatusShouldBe(string status)
        {
            _result.Success.Should().BeTrue(_result.Message);
            var empId = _result.Data.Id;
            var employee = await _dbContext.Employees.FindAsync(empId);
            employee.Status.ToString().Should().Be(status);
            
            // Should be the SAME employee record
            empId.Should().Be(_draftEmployeeId);
        }

        [Then(@"the employee code should generate a permanent ""(.*)"" code")]
        public async Task ThenTheEmployeeCodeShouldGenerateAPermanentCode(string prefix)
        {
             var employee = await _dbContext.Employees.FindAsync(_draftEmployeeId);
             employee.EmployeeCode.Should().StartWith(prefix);
             employee.EmployeeCode.Should().NotStartWith("DRAFT");
        }

        [Then(@"a salary record should be created for the employee")]
        public async Task ThenASalaryRecordShouldBeCreatedForTheEmployee()
        {
             var salary = await _dbContext.Salaries.FirstOrDefaultAsync(s => s.EmployeeId == _draftEmployeeId);
             salary.Should().NotBeNull();
             salary.BasicSalary.Should().Be(50000);
        }

        [Then(@"the offer letter status should be ""(.*)""")]
        public async Task ThenTheOfferLetterStatusShouldBe(string status)
        {
            var offer = await _dbContext.OfferLetters.FindAsync(_offerLetterId);
            offer.Status.ToString().Should().Be(status);
        }

        [Then(@"the acceptance should fail with error ""(.*)""")]
        public void ThenTheAcceptanceShouldFailWithError(string error)
        {
            _result.Success.Should().BeFalse();
            _result.Message.Should().Contain(error);
        }
    }
}
