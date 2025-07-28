using AutoMapper;
using Project.DTOs;
using Project.Models;

namespace Project.MappingProfile
{
    public class RouteProfile:Profile
    {
        public RouteProfile() {
            
               CreateMap<Models.Route, RouteDTO>()
    .ForMember(dest => dest.Segments, opt => opt.MapFrom(src => src.RouteSegments.OrderBy(s => s.SegmentId)))
    .ForMember(dest => dest.DepartureStationName, opt => opt.MapFrom(src => src.DepartureStation.StationName))
    .ForMember(dest => dest.ArrivalStationName, opt => opt.MapFrom(src => src.ArrivalStation.StationName));

            CreateMap<RouteDTO, Models.Route>().ForMember(dest => dest.RouteSegments, opt => opt.MapFrom(src => src.Segments));

            CreateMap<RouteSegment, RouteSegmentDTO>()
                .ForMember(dest => dest.FromStationName, opt => opt.MapFrom(src => src.FromStation.StationName))
                .ForMember(dest => dest.ToStationName, opt => opt.MapFrom(src => src.ToStation.StationName));

            // RouteSegmentDTO -> RouteSegment
            CreateMap<RouteSegmentDTO, RouteSegment>();

        }

        
    }
}
