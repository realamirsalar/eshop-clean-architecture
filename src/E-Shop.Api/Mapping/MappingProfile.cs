using AutoMapper;
using E_Shop.Api.Contracts.Category;
using E_Shop.Api.Contracts.Product;
using E_Shop.Application.Features.Category.Commands;
using E_Shop.Application.Features.Product.Commands;

namespace E_Shop.Api.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateCategoryRequest, CreateCategoryCommand>();
            CreateMap<UpdateCategoryRequest, UpdateCategoryCommand>();


            CreateMap<CreateProductRequest, CreateProductCommand>();
            //CreateMap<UpdateProductRequest, UpdateProductCommand>();
        }
    }
}
