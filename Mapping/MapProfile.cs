using AutoMapper;
using KDemia.Models;
using KDemia.ViewModels;

namespace KDemia.MappingProfiles
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<Course, CourseModel>().ReverseMap();
        }
    }
}
