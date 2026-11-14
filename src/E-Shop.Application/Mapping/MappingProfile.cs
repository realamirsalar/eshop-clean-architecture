
using AutoMapper;
using E_Shop.Application.Features.Category.Commands;
using E_Shop.Application.Features.Category.Queries;
using E_Shop.Application.Features.Product.Commands;
using E_Shop.Application.Features.Product.Queries;
using E_Shop.Domain;

namespace E_Shop.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateCategoryCommand, Category>();
            CreateMap<UpdateCategoryCommand, Category>();
            CreateMap<Category, CategoryResponse>();


            CreateMap<CreateProductCommand, Product>();
            //CreateMap<UpdateProductCommand, Product>();
            CreateMap<Product, ProductResponse>();
        }
    }
}
