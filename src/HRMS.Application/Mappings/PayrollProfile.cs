using AutoMapper;
using HRMS.Application.DTOs.Payroll;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class PayrollProfile : Profile
{
    public PayrollProfile()
    {
        CreateMap<Payroll, PayrollDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FullName))
            .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee.EmployeeCode))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Employee.Department.Name))
            .ForMember(dest => dest.DesignationTitle, opt => opt.MapFrom(src => src.Employee.Designation.Title));
    }
}
