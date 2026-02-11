using AutoMapper;
using HRMS.Application.DTOs.Employee;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class EmployeeProfile : Profile
{
    public EmployeeProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
            .ForMember(dest => dest.DesignationTitle, opt => opt.MapFrom(src => src.Designation.Title))
            .ForMember(dest => dest.ReportingManagerName, opt => opt.MapFrom(src => src.ReportingManager != null ? src.ReportingManager.FullName : null))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));  // Map as int

        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => (Domain.Enums.Gender)src.Gender))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.EmployeeStatus.Draft));

        CreateMap<UpdateEmployeeDto, Employee>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => (Domain.Enums.Gender)src.Gender))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (Domain.Enums.EmployeeStatus)src.Status));
    }
}
