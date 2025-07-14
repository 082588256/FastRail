using AutoMapper;
using Project.Constants.Enums;
using Project.DTOs;
using Project.Models;

namespace Project.MappingProfile
{
    public class CarriageProfile:Profile
    {
        public CarriageProfile()
        {
            CreateMap<Carriage, CarriageDto>()
                .ForMember(dest => dest.CarriageType, opt => opt.MapFrom(src => Enum.Parse<CarriageType>(src.CarriageType)))
                .ReverseMap()
                .ForMember(dest => dest.CarriageType, opt => opt.MapFrom(src => src.CarriageType.ToString()));

            CreateMap<Seat, SeatDto>()
                .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => Enum.Parse<SeatType>(src.SeatType)))
                .ReverseMap()
                .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => src.SeatType.ToString()));
        }

    }
}
