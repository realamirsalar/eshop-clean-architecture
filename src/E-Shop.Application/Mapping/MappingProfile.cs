
using AutoMapper;
using E_Shop.Application.Features.Category.Commands;
using E_Shop.Application.Features.Category.Queries;
using E_Shop.Domain;

namespace E_Shop.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category , CategoryResponse>();
            CreateMap<CreateCategoryCommand, Category>();
        }
    }
}
