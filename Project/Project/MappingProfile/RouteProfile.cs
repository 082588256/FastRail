using AutoMapper;
using Project.DTOs;
using Project.Models;

namespace Project.MappingProfile
{
    public class RouteProfile:Profile
    {
        public RouteProfile() {
            CreateMap<Models.Route, RouteDTO>()
               .ForMember(dest => dest.Segments, opt => opt.MapFrom(src => src.Segments.OrderBy(s => s.SegmentId)))
               .ReverseMap();

            CreateMap<RouteSegment, RouteSegmentDTO>().ReverseMap();
            
        }

        
    }
}
