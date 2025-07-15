using AutoMapper;
using Project.DTOs;
using Project.Models;

namespace Project.MappingProfile
{
    public class RouteProfile:Profile
    {
        public RouteProfile() {
            CreateMap<Models.Route, RouteDTO>()
               .ForMember(dest => dest.Segments, opt => opt.MapFrom(src => src.RouteSegments.OrderBy(s => s.SegmentId)));

            CreateMap<RouteDTO, Models.Route>().ForMember(dest => dest.RouteSegments, opt => opt.MapFrom(src => src.Segments));

            CreateMap<RouteSegment, RouteSegmentDTO>().ReverseMap();
            
        }

        
    }
}
