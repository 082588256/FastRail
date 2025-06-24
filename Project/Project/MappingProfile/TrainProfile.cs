using AutoMapper;
using Project.DTOs;
using Project.Models;

namespace Project.MappingProfile
{
    public class TrainProfile:Profile
    {
        public TrainProfile() {
            CreateMap<Train, TrainDTO>().ReverseMap();
        }
    }
}
