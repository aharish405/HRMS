## WorkAxis HRMS

Enterprise-grade Human Resource Management System built with ASP.NET Core MVC (.NET 8).

### Features
- Employee Management
- Payroll Processing
- Leave Management
- Offer Letter Generation
- Master Data Management
- Role-based Access Control

### Tech Stack
- ASP.NET Core MVC (.NET 8)
- SQL Server
- Entity Framework Core
- ASP.NET Core Identity
- Bootstrap 5
- Serilog

### Getting Started

1. Update connection string in `appsettings.json`
2. Run migrations: `dotnet ef database update`
3. Run the application: `dotnet run`
4. Login with: admin@workaxis.com / Admin@12345

### Architecture
Clean Architecture with 5 layers:
- Domain
- Application
- Infrastructure
- Web
- Shared
