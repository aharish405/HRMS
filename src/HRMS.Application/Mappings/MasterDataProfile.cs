using AutoMapper;
using HRMS.Application.DTOs.Department;
using HRMS.Application.DTOs.Designation;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class MasterDataProfile : Profile
{
    public MasterDataProfile()
    {
        // Department mappings
        CreateMap<Department, DepartmentDto>()
            .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees.Count));
        CreateMap<CreateDepartmentDto, Department>();

        // Designation mappings
        CreateMap<Designation, DesignationDto>()
            .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees.Count));
        CreateMap<CreateDesignationDto, Designation>();
    }
}
