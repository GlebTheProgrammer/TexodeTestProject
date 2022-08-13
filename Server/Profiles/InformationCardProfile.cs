using AutoMapper;
using Server.DTOs;
using Server.Models;

namespace Server.Profiles
{
    public class InformationCardProfile : Profile
    {
        public InformationCardProfile()
        {
            // Source -> Target

            CreateMap<InformationCard, InformationCardReadDto>();
            CreateMap<InformationCardCreateDto, InformationCard>();
            CreateMap<InformationCardUpdateDto, InformationCard>();
        }
    }
}
