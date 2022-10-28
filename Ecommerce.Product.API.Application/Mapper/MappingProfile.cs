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
            #region Request
            CreateMap<ProductRequestModel, ProductModel>();
            #endregion

            #region Response
            CreateMap<ProductModel, ProductResponseModel>();
            #endregion
        }
    }
}
