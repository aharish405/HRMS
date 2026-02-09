using AutoMapper;
using HRMS.Application.DTOs.Leave;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Mappings;

public class LeaveProfile : Profile
{
    public LeaveProfile()
    {
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FullName))
            .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee.EmployeeCode))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Employee.Department.Name))
            .ForMember(dest => dest.LeaveTypeName, opt => opt.MapFrom(src => src.LeaveType.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.NumberOfDays, opt => opt.MapFrom(src => (int)src.NumberOfDays));

        CreateMap<LeaveBalance, LeaveBalanceDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FullName))
            .ForMember(dest => dest.LeaveTypeName, opt => opt.MapFrom(src => src.LeaveType.Name));
    }
}
