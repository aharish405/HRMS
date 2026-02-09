using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Identity;
using HRMS.Infrastructure.Repositories;
using HRMS.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using HRMS.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("HRMS.Infrastructure")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Register repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(HRMS.Application.Mappings.EmployeeProfile));

// Register Application Services
builder.Services.AddScoped<HRMS.Application.Interfaces.IEmployeeService, HRMS.Application.Services.EmployeeService>();
builder.Services.AddScoped<HRMS.Application.Interfaces.IMasterDataService, HRMS.Application.Services.MasterDataService>();
builder.Services.AddScoped<HRMS.Application.Interfaces.ISalaryService, HRMS.Application.Services.SalaryService>();
builder.Services.AddScoped<HRMS.Application.Interfaces.IPayrollService, HRMS.Application.Services.PayrollService>();
builder.Services.AddScoped<HRMS.Application.Interfaces.ILeaveService, HRMS.Application.Services.LeaveService>();
builder.Services.AddScoped<HRMS.Application.Interfaces.IOfferLetterService, HRMS.Application.Services.OfferLetterService>();

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.CanManageEmployees, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.HRAdmin));

    options.AddPolicy(Policies.CanProcessPayroll, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.HRAdmin));

    options.AddPolicy(Policies.CanApproveLeaves, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.HRAdmin, Roles.Manager));

    options.AddPolicy(Policies.CanGenerateOfferLetters, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.HRAdmin));

    options.AddPolicy(Policies.CanManageMasterData, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.HRAdmin));

    options.AddPolicy(Policies.CanViewReports, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.HRAdmin));
});

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Session (if needed)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseSerilogRequestLogging();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    Log.Information("Starting WorkAxis HRMS application");

    // Seed database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            await DbInitializer.SeedAsync(context, userManager, roleManager);
            Log.Information("Database seeded successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while seeding the database");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
