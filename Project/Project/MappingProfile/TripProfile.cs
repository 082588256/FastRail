using AutoMapper;
using Project.DTOs;
using Project.Models;

namespace Project.MappingProfile
{
    public class TripProfile : Profile
    {
        public TripProfile()
        {

            CreateMap<Trip, TripDTO>()
            .ForMember(dest => dest.TrainName, opt => opt.MapFrom(src => src.Train.Name))
            .ForMember(dest => dest.RouteName, opt => opt.MapFrom(src => src.Route.RouteName));

            CreateMap<TripDTO, Trip>();
        }

    }
}
