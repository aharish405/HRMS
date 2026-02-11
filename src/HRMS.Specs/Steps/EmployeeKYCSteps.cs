using FluentAssertions;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
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
    public class EmployeeKYCSteps : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly ScenarioContext _scenarioContext;
        private IServiceScope _scope;
        private IEmployeeService _employeeService;
        private CreateEmployeeDto _createDto;
        private Result<EmployeeDto> _result;
        private string _userName = "test.user@workaxis.com";

        public EmployeeKYCSteps(CustomWebApplicationFactory<Program> factory, ScenarioContext scenarioContext)
        {
            _factory = factory;
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void Setup()
        {
            _scope = _factory.Services.CreateScope();
            _employeeService = _scope.ServiceProvider.GetRequiredService<IEmployeeService>();
            
            // Seed Master Data for Tests
            var db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (!db.Departments.Any(d => d.Id == 1))
            {
                db.Departments.Add(new Department { Id = 1, Name = "IT", Code = "IT", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 } });
            }
            if (!db.Designations.Any(d => d.Id == 1))
            {
                db.Designations.Add(new Designation { Id = 1, Title = "Developer", Code = "DEV", Level = 1, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 } });
            }
            db.SaveChanges();
        }

        [AfterScenario]
        public void Teardown()
        {
            _scope?.Dispose();
        }

        [Given(@"I have a new employee draft with the following details")]
        public void GivenIHaveANewEmployeeDraftWithTheFollowingDetails(Table table)
        {
            var row = table.Rows[0];
            _createDto = new CreateEmployeeDto
            {
                FirstName = row["FirstName"],
                LastName = row["LastName"],
                Email = row["Email"],
                Phone = row["Phone"],
                EmployeeCode = $"TEST_{Guid.NewGuid().ToString().Substring(0, 8)}", // Unique for each test run
                DepartmentId = 1, // Assumed valid ID for test
                DesignationId = 1, // Assumed valid ID for test
                ReportingManagerId = null,
                DateOfBirth = DateTime.Today.AddYears(-30),
                JoiningDate = DateTime.Today,
                Gender = (int)Gender.Male, 
                Address = "123 Test St",
                // Status is removed from DTO, defaults to Draft in logic
            };
        }

        [Given(@"I provide the following KYC details")]
        public void GivenIProvideTheFollowingKYCDetails(Table table)
        {
            var row = table.Rows[0];
            _createDto.PanNumber = row["PanNumber"];
            _createDto.AadharNumber = row["AadharNumber"];
            if (table.Header.Contains("EmergencyContact"))
            {
                _createDto.EmergencyContactName = row["EmergencyContact"];
            }
            if (table.Header.Contains("EmergencyPhone"))
            {
                _createDto.EmergencyContactPhone = row["EmergencyPhone"];
            }
            _createDto.BloodGroup = "O+"; // Default for test
        }

        [When(@"I submit the create employee form")]
        public async Task WhenISubmitTheCreateEmployeeForm()
        {
            _result = await _employeeService.CreateEmployeeAsync(_createDto, _userName);
        }

        [Then(@"the employee should be created successfully")]
        public void ThenTheEmployeeShouldBeCreatedSuccessfully()
        {
            if (!_result.Success)
            {
                Console.WriteLine($"Create Employee Failed: {_result.Message}");
                if (_result.Errors != null)
                {
                    foreach (var error in _result.Errors)
                    {
                        Console.WriteLine($"Error: {error}");
                    }
                }
            }
            _result.Success.Should().BeTrue($"because create should succeed but failed with: {_result.Message}");
            _result.Data.Should().NotBeNull();
            _result.Data.Id.Should().BeGreaterThan(0);
        }

        [Then(@"the saved employee should have the correct KYC details")]
        public async Task ThenTheSavedEmployeeShouldHaveTheCorrectKYCDetails(Table table)
        {
            var row = table.Rows[0];
            var employeeId = _result.Data.Id;
            
            var savedEmployeeResult = await _employeeService.GetEmployeeByIdAsync(employeeId);
            savedEmployeeResult.Success.Should().BeTrue();
            
            var savedEmployee = savedEmployeeResult.Data;
            savedEmployee.PanNumber.Should().Be(row["PanNumber"]);
            savedEmployee.AadharNumber.Should().Be(row["AadharNumber"]);
        }

        [Then(@"the employee status should be 'Draft'")]
        public async Task ThenTheEmployeeStatusShouldBeDraft()
        {
            var employeeId = _result.Data.Id;
            var savedEmployeeResult = await _employeeService.GetEmployeeByIdAsync(employeeId);
            
            ((EmployeeStatus)savedEmployeeResult.Data.Status).Should().Be(EmployeeStatus.Draft);
        }
    }
}
