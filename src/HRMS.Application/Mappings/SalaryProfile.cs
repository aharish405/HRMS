using AutoMapper;
using HRMS.Application.DTOs.Salary;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class SalaryProfile : Profile
{
    public SalaryProfile()
    {
        CreateMap<Salary, SalaryDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FullName))
            .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee.EmployeeCode));

        CreateMap<CreateSalaryDto, Salary>()
            .ForMember(dest => dest.GrossSalary, opt => opt.Ignore())
            .ForMember(dest => dest.TotalDeductions, opt => opt.Ignore())
            .ForMember(dest => dest.NetSalary, opt => opt.Ignore())
            .ForMember(dest => dest.CTC, opt => opt.Ignore());
    }
}
