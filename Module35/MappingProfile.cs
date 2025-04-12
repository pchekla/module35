using AutoMapper;
using Module35.Models;
using Module35.ViewModels;

namespace Module35;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Маппинг из RegisterViewModel в User
        CreateMap<RegisterViewModel, User>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailReg))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Login))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => new DateTime(src.Year, src.Month, src.Date)));
        
        // Обратный маппинг из User в RegisterViewModel
        CreateMap<User, RegisterViewModel>()
            .ForMember(dest => dest.EmailReg, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Login, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.BirthDate.Day))
            .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.BirthDate.Month))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.BirthDate.Year));

        // Маппинг из LoginViewModel в User
        CreateMap<LoginViewModel, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
    }
} 