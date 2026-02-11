using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        // Ensure database is created
        // await context.Database.MigrateAsync();
        // FORCE RESET disabled to persist data
        // await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Admin User
        await SeedAdminUserAsync(userManager);

        // Seed Master Data
        await SeedMasterDataAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roles = { "SuperAdmin", "HRAdmin", "Employee", "Manager" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Name = roleName,
                    Description = $"{roleName} role"
                };
                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@workaxis.com";

        var existingUser = await userManager.FindByEmailAsync(adminEmail);
        if (existingUser == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Administrator",
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@12345");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }
        }
    }

    private static async Task SeedMasterDataAsync(ApplicationDbContext context)
    {
        // Seed Departments
        if (!await context.Departments.AnyAsync())
        {
            var departments = new List<Department>
            {
                new Department { Name = "Information Technology", Code = "IT", Description = "IT Department", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Department { Name = "Human Resources", Code = "HR", Description = "HR Department", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Department { Name = "Finance", Code = "FIN", Description = "Finance Department", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Department { Name = "Sales", Code = "SAL", Description = "Sales Department", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Department { Name = "Marketing", Code = "MKT", Description = "Marketing Department", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow }
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }

        // Seed Designations
        if (!await context.Designations.AnyAsync())
        {
            var designations = new List<Designation>
            {
                new Designation { Title = "Software Engineer", Code = "SE", Level = 1, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Designation { Title = "Senior Software Engineer", Code = "SSE", Level = 2, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Designation { Title = "Team Lead", Code = "TL", Level = 3, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Designation { Title = "Project Manager", Code = "PM", Level = 4, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Designation { Title = "HR Manager", Code = "HRM", Level = 4, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Designation { Title = "Finance Manager", Code = "FM", Level = 4, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow }
            };

            await context.Designations.AddRangeAsync(designations);
            await context.SaveChangesAsync();
        }

        // Seed Leave Types
        if (!await context.LeaveTypes.AnyAsync())
        {
            var leaveTypes = new List<LeaveType>
            {
                new LeaveType { Name = "Casual Leave", Type = LeaveTypeEnum.CasualLeave, DefaultDaysPerYear = 12, IsActive = true, IsPaid = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new LeaveType { Name = "Sick Leave", Type = LeaveTypeEnum.SickLeave, DefaultDaysPerYear = 12, IsActive = true, IsPaid = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new LeaveType { Name = "Earned Leave", Type = LeaveTypeEnum.EarnedLeave, DefaultDaysPerYear = 15, IsActive = true, IsPaid = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new LeaveType { Name = "Loss of Pay", Type = LeaveTypeEnum.LossOfPay, DefaultDaysPerYear = 0, IsActive = true, IsPaid = false, CreatedBy = "System", CreatedOn = DateTime.UtcNow }
            };

            await context.LeaveTypes.AddRangeAsync(leaveTypes);
            await context.SaveChangesAsync();
        }

        // Seed Template Placeholders
        await SeedTemplatePlaceholdersAsync(context);

        // Seed Default Offer Letter Template
        await SeedOfferLetterTemplatesAsync(context);
    }

    private static async Task SeedTemplatePlaceholdersAsync(ApplicationDbContext context)
    {
        if (!await context.TemplatePlaceholders.AnyAsync())
        {
            var placeholders = new List<TemplatePlaceholder>
            {
                // Candidate Information
                new TemplatePlaceholder { PlaceholderKey = "CANDIDATE_NAME", DisplayName = "Candidate Name", Category = PlaceholderCategory.Candidate, DataType = "string", Description = "Full name of the candidate", SampleValue = "John Doe", IsRequired = true, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "CANDIDATE_EMAIL", DisplayName = "Candidate Email", Category = PlaceholderCategory.Candidate, DataType = "string", Description = "Email address of the candidate", SampleValue = "john.doe@example.com", IsRequired = true, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "CANDIDATE_PHONE", DisplayName = "Candidate Phone", Category = PlaceholderCategory.Candidate, DataType = "string", Description = "Phone number of the candidate", SampleValue = "+91 9876543210", IsRequired = false, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "CANDIDATE_ADDRESS", DisplayName = "Candidate Address", Category = PlaceholderCategory.Candidate, DataType = "string", Description = "Address of the candidate", SampleValue = "123 Main Street, City, State - 123456", IsRequired = false, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },

                // Company Information
                new TemplatePlaceholder { PlaceholderKey = "COMPANY_NAME", DisplayName = "Company Name", Category = PlaceholderCategory.Company, DataType = "string", Description = "Name of the company", SampleValue = "WorkAxis Technologies", IsRequired = true, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "COMPANY_ADDRESS", DisplayName = "Company Address", Category = PlaceholderCategory.Company, DataType = "string", Description = "Company address", SampleValue = "456 Business Park, Tech City - 654321", IsRequired = false, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "OFFER_DATE", DisplayName = "Offer Date", Category = PlaceholderCategory.Company, DataType = "date", Description = "Date of offer letter", SampleValue = DateTime.Now.ToString("dd MMMM yyyy"), FormatString = "dd MMMM yyyy", IsRequired = true, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },

                // Position Details
                new TemplatePlaceholder { PlaceholderKey = "DESIGNATION", DisplayName = "Designation", Category = PlaceholderCategory.Position, DataType = "string", Description = "Job title/designation", SampleValue = "Senior Software Engineer", IsRequired = true, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "DEPARTMENT", DisplayName = "Department", Category = PlaceholderCategory.Position, DataType = "string", Description = "Department name", SampleValue = "Information Technology", IsRequired = true, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "JOINING_DATE", DisplayName = "Joining Date", Category = PlaceholderCategory.Position, DataType = "date", Description = "Expected date of joining", SampleValue = DateTime.Now.AddDays(30).ToString("dd MMMM yyyy"), FormatString = "dd MMMM yyyy", IsRequired = true, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "LOCATION", DisplayName = "Work Location", Category = PlaceholderCategory.Position, DataType = "string", Description = "Work location", SampleValue = "Bangalore", IsRequired = true, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },

                // Salary Details
                new TemplatePlaceholder { PlaceholderKey = "BASIC_SALARY", DisplayName = "Basic Salary", Category = PlaceholderCategory.Salary, DataType = "currency", Description = "Basic salary amount", SampleValue = "50,000", FormatString = "N2", IsRequired = true, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "HRA", DisplayName = "House Rent Allowance", Category = PlaceholderCategory.Salary, DataType = "currency", Description = "HRA amount", SampleValue = "25,000", FormatString = "N2", IsRequired = false, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "CONVEYANCE_ALLOWANCE", DisplayName = "Conveyance Allowance", Category = PlaceholderCategory.Salary, DataType = "currency", Description = "Conveyance allowance", SampleValue = "1,600", FormatString = "N2", IsRequired = false, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "MEDICAL_ALLOWANCE", DisplayName = "Medical Allowance", Category = PlaceholderCategory.Salary, DataType = "currency", Description = "Medical allowance", SampleValue = "1,250", FormatString = "N2", IsRequired = false, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "SPECIAL_ALLOWANCE", DisplayName = "Special Allowance", Category = PlaceholderCategory.Salary, DataType = "currency", Description = "Special allowance", SampleValue = "15,000", FormatString = "N2", IsRequired = false, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "OTHER_ALLOWANCES", DisplayName = "Other Allowances", Category = PlaceholderCategory.Salary, DataType = "currency", Description = "Other allowances", SampleValue = "5,000", FormatString = "N2", IsRequired = false, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "CTC", DisplayName = "Cost to Company (Annual)", Category = PlaceholderCategory.Salary, DataType = "currency", Description = "Total CTC per annum", SampleValue = "12,00,000", FormatString = "N2", IsRequired = true, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new TemplatePlaceholder { PlaceholderKey = "GROSS_SALARY", DisplayName = "Gross Salary (Monthly)", Category = PlaceholderCategory.Salary, DataType = "currency", Description = "Monthly gross salary", SampleValue = "97,850", FormatString = "N2", IsRequired = false, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow }
            };

            await context.TemplatePlaceholders.AddRangeAsync(placeholders);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedOfferLetterTemplatesAsync(ApplicationDbContext context)
    {
        if (!await context.OfferLetterTemplates.AnyAsync())
        {
            var defaultTemplate = new OfferLetterTemplate
            {
                Name = "Standard Offer Letter Template",
                Description = "Default offer letter template with all standard clauses and salary structure",
                Category = "Standard",
                IsActive = true,
                IsDefault = true,
                Version = 1,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow,
                Content = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: 'Arial', sans-serif; line-height: 1.6; color: #333; margin: 40px; }
        .header { text-align: center; margin-bottom: 30px; border-bottom: 2px solid #0061f2; padding-bottom: 20px; }
        .company-name { font-size: 24px; font-weight: bold; color: #0061f2; }
        .letter-date { text-align: right; margin: 20px 0; }
        .subject { font-weight: bold; margin: 20px 0; text-decoration: underline; }
        .content { margin: 20px 0; text-align: justify; }
        .salary-table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        .salary-table th, .salary-table td { border: 1px solid #ddd; padding: 12px; text-align: left; }
        .salary-table th { background-color: #0061f2; color: white; }
        .salary-table .total-row { background-color: #f8f9fa; font-weight: bold; }
        .clause { margin: 15px 0; }
        .clause-title { font-weight: bold; margin-bottom: 5px; }
        .signature-section { margin-top: 50px; }
        .signature-box { display: inline-block; width: 45%; margin-top: 80px; }
        .page-break { page-break-after: always; }
    </style>
</head>
<body>
    <!-- Page 1: Header and Introduction -->
    <div class=""header"">
        <div class=""company-name"">{{COMPANY_NAME}}</div>
        <div>{{COMPANY_ADDRESS}}</div>
    </div>

    <div class=""letter-date"">
        Date: {{OFFER_DATE}}
    </div>

    <div>
        <strong>{{CANDIDATE_NAME}}</strong><br>
        {{CANDIDATE_ADDRESS}}<br>
        Email: {{CANDIDATE_EMAIL}}<br>
        Phone: {{CANDIDATE_PHONE}}
    </div>

    <div class=""subject"">
        Subject: Offer of Employment - {{DESIGNATION}}
    </div>

    <div class=""content"">
        <p>Dear {{CANDIDATE_NAME}},</p>

        <p>We are pleased to offer you the position of <strong>{{DESIGNATION}}</strong> in the <strong>{{DEPARTMENT}}</strong> department at {{COMPANY_NAME}}. We believe that your skills and experience will be a valuable addition to our team.</p>

        <p>This offer is contingent upon successful completion of background verification and submission of required documents.</p>
    </div>

    <!-- Page 2: Terms and Conditions -->
    <div class=""page-break""></div>

    <h3>Terms and Conditions of Employment</h3>

    <div class=""clause"">
        <div class=""clause-title"">1. Position and Department</div>
        <p>You will be employed as <strong>{{DESIGNATION}}</strong> in the <strong>{{DEPARTMENT}}</strong> department, reporting to the designated supervisor/manager.</p>
    </div>

    <div class=""clause"">
        <div class=""clause-title"">2. Date of Joining</div>
        <p>Your employment will commence on <strong>{{JOINING_DATE}}</strong>. You are requested to report to our <strong>{{LOCATION}}</strong> office at 10:00 AM on the joining date.</p>
    </div>

    <div class=""clause"">
        <div class=""clause-title"">3. Probation Period</div>
        <p>You will be on probation for a period of 6 (six) months from your date of joining. During this period, your performance will be reviewed, and upon satisfactory completion, you will be confirmed in your position. Either party may terminate the employment during probation with 15 days' notice or payment in lieu thereof.</p>
    </div>

    <div class=""clause"">
        <div class=""clause-title"">4. Working Hours</div>
        <p>Your normal working hours will be 9:30 AM to 6:30 PM, Monday through Friday, with a one-hour lunch break. You may be required to work additional hours as per business requirements.</p>
    </div>

    <!-- Page 3: More Clauses -->
    <div class=""page-break""></div>

    <div class=""clause"">
        <div class=""clause-title"">5. Leave Entitlement</div>
        <p>You will be entitled to the following leave benefits as per company policy:</p>
        <ul>
            <li>Casual Leave: 12 days per year</li>
            <li>Sick Leave: 12 days per year</li>
            <li>Earned Leave: 15 days per year</li>
            <li>Public Holidays: As per company calendar</li>
        </ul>
    </div>

    <div class=""clause"">
        <div class=""clause-title"">6. Confidentiality and Non-Disclosure</div>
        <p>You agree to maintain strict confidentiality regarding all company information, trade secrets, client data, and proprietary information. This obligation continues even after termination of employment.</p>
    </div>

    <div class=""clause"">
        <div class=""clause-title"">7. Intellectual Property</div>
        <p>All work products, inventions, and intellectual property created during your employment shall be the exclusive property of {{COMPANY_NAME}}.</p>
    </div>

    <div class=""clause"">
        <div class=""clause-title"">8. Termination</div>
        <p>After successful completion of probation, either party may terminate this employment with 60 days' written notice or payment in lieu thereof. The company reserves the right to terminate employment immediately in case of misconduct or breach of company policies.</p>
    </div>

    <!-- Page 4: Additional Clauses -->
    <div class=""page-break""></div>

    <div class=""clause"">
        <div class=""clause-title"">9. Code of Conduct</div>
        <p>You are expected to adhere to the company's code of conduct, policies, and procedures at all times. Any violation may result in disciplinary action, including termination.</p>
    </div>

    <div class=""clause"">
        <div class=""clause-title"">10. Background Verification</div>
        <p>This offer is subject to satisfactory verification of your educational qualifications, employment history, and background check. Any discrepancy found may result in withdrawal of this offer or termination of employment.</p>
    </div>

    <div class=""clause"">
        <div class=""clause-title"">11. Documents Required</div>
        <p>Please submit the following documents on your joining date:</p>
        <ul>
            <li>Educational certificates and mark sheets</li>
            <li>Previous employment relieving letters and experience certificates</li>
            <li>PAN card and Aadhaar card copies</li>
            <li>Passport size photographs (4 copies)</li>
            <li>Bank account details and canceled cheque</li>
            <li>Any other documents as requested by HR</li>
        </ul>
    </div>

    <!-- Page 5: Salary Structure -->
    <div class=""page-break""></div>

    <h3>Compensation Structure</h3>

    <p>Your annual Cost to Company (CTC) will be <strong>₹{{CTC}}</strong>, with the following monthly breakup:</p>

    <table class=""salary-table"">
        <thead>
            <tr>
                <th>Component</th>
                <th>Amount (₹)</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>Basic Salary</td>
                <td>{{BASIC_SALARY}}</td>
            </tr>
            <tr>
                <td>House Rent Allowance (HRA)</td>
                <td>{{HRA}}</td>
            </tr>
            <tr>
                <td>Conveyance Allowance</td>
                <td>{{CONVEYANCE_ALLOWANCE}}</td>
            </tr>
            <tr>
                <td>Medical Allowance</td>
                <td>{{MEDICAL_ALLOWANCE}}</td>
            </tr>
            <tr>
                <td>Special Allowance</td>
                <td>{{SPECIAL_ALLOWANCE}}</td>
            </tr>
            <tr>
                <td>Other Allowances</td>
                <td>{{OTHER_ALLOWANCES}}</td>
            </tr>
            <tr class=""total-row"">
                <td>Gross Salary (Monthly)</td>
                <td>{{GROSS_SALARY}}</td>
            </tr>
        </tbody>
    </table>

    <p><strong>Note:</strong> The above salary is subject to applicable tax deductions as per Income Tax Act. Provident Fund (PF) and other statutory deductions will be made as per government regulations.</p>

    <!-- Page 6: Acceptance and Signature -->
    <div class=""page-break""></div>

    <div class=""content"">
        <p>We are excited to have you join our team and look forward to a mutually beneficial association. Please sign and return a copy of this letter to indicate your acceptance of this offer.</p>

        <p>If you have any questions regarding this offer, please feel free to contact our HR department.</p>

        <p>Welcome to {{COMPANY_NAME}}!</p>
    </div>

    <div class=""signature-section"">
        <div class=""signature-box"">
            <div>_______________________</div>
            <div><strong>For {{COMPANY_NAME}}</strong></div>
            <div>HR Manager</div>
            <div>Date: {{OFFER_DATE}}</div>
        </div>

        <div class=""signature-box"" style=""float: right;"">
            <div>_______________________</div>
            <div><strong>{{CANDIDATE_NAME}}</strong></div>
            <div>Candidate Signature</div>
            <div>Date: _______________</div>
        </div>
    </div>

    <div style=""clear: both; margin-top: 50px;"">
        <p style=""font-size: 12px; color: #666; text-align: center;"">
            This is a computer-generated offer letter and does not require a physical signature from the company.
        </p>
    </div>
</body>
</html>"
            };

            await context.OfferLetterTemplates.AddAsync(defaultTemplate);
            await context.SaveChangesAsync();
        }
    }
}
