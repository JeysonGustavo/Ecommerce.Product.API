using AutoMapper;
using Ecommerce.Product.API.Core.Models.Domain;
using Ecommerce.Product.API.Core.Models.Request;
using Ecommerce.Product.API.Core.Models.Response;

namespace Ecommerce.Product.API.Application.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductModel, ProductResponseModel>();
            CreateMap<ProductRequestModel, ProductModel> ();
        }
    }
}
