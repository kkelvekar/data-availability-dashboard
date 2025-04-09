using AutoMapper;
using DaDashboard.Domain;
using DaDashboard.API.DTO;

namespace DaDashboard.API.MappingProfiles
{
    public class BusinessEntitySummaryProfile : Profile
    {
        public BusinessEntitySummaryProfile()
        {
            CreateMap<BusinessEntitySummary, BusinessEntitySummaryResponse>()
                .ForPath(dest => dest.Status.Indicator, opt => opt.MapFrom(src => src.Status.Indicator))
                .ForPath(dest => dest.Status.Description, opt => opt.MapFrom(src => src.Status.Description));
        }
    }
}
