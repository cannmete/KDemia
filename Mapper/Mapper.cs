using AutoMapper;
using KDemia.Models; 
using KDemia.ViewModels; 


namespace KDemia.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterViewModel, User>();
                //.ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                //.ForMember(dest => dest.Salt, opt => opt.Ignore());
        }
    }
}


