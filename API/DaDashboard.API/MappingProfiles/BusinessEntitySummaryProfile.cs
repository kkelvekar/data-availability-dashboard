using AutoMapper;
using DaDashboard.API.DTO;
using DaDashboard.Domain;

namespace DaDashboard.API.MappingProfiles
{
    public class BusinessEntitySummaryProfile : Profile
    {
        public BusinessEntitySummaryProfile()
        {
            CreateMap<BusinessEntitySummary, BusinessEntitySummaryResponse>()
                // Map BusinessEntityID to Id.
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BusinessEntityID))
                // Initialize DependentFuncs with an empty list.
                .ForMember(dest => dest.DependentFuncs, opt => opt.MapFrom(src => new List<string>()))
                // Initialize Status with default values (e.g., Green and an empty description).
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => new EntityStatus
                {
                    Indicator = RagIndicator.Green,
                    Description = string.Empty
                }));
        }
    }
}
