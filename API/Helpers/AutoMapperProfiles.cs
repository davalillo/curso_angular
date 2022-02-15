using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(
                dest => dest.PhotoUrl,
                opt => opt.MapFrom(src => src.Photos.FirstOrDefault<Photo>(x => x.IsMain).Url))
            .ForMember(dest => dest.Age,
                opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge())
                )
                ;
        CreateMap<Photo, PhotoDto>().ReverseMap();

        CreateMap<MemberUpdateDto, AppUser>();

        CreateMap<RegisterDto, AppUser>();

        CreateMap<Message, MessageDto>()
            .ForMember(dest=>dest.SenderPhotoUrl, opt=>opt.MapFrom(source=>source.Sender.Photos.FirstOrDefault(x=>x.IsMain).Url))
            .ForMember(dest=>dest.RecipientPhotoUrl, opt=>opt.MapFrom(source=>source.Recipient.Photos.FirstOrDefault(x=>x.IsMain).Url));
    }
}
