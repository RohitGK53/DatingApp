using System.Linq;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
           CreateMap<User, UserForListDto>()
           .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
           .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
           CreateMap<User, UserForDetailedDto>()
           .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
           .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
           CreateMap<Photo, PhotosForDetailedDto>();
           CreateMap<UserForUpdDto, User>();
           CreateMap<Photo, PhotoForReturnDto>();
           CreateMap<PhotoForCreationDto, Photo>();
           CreateMap<UserForRegister, User>();
           CreateMap<MessageForCreationDto,Message>().ReverseMap();
           CreateMap<Message,MessageToReturnDto>()
           .ForMember(m => m.SenderPhotoUrl , opy => opy.MapFrom(u => u.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
           .ForMember(m => m.RecipientPhotoUrl , opy => opy.MapFrom(u => u.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));

        }
    }
}