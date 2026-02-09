using AutoMapper;
using HRMS.Application.DTOs.OfferLetter;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class OfferLetterProfile : Profile
{
    public OfferLetterProfile()
    {
        CreateMap<OfferLetter, OfferLetterDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
            .ForMember(dest => dest.DesignationTitle, opt => opt.MapFrom(src => src.Designation.Title));
    }
}
