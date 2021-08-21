using System;
using AutoMapper;

namespace ShoppingCartGrpc.Mapper
{
    public class ShoppingCartProfile : Profile
    {
        public ShoppingCartProfile()
        {
            CreateMap<Models.ShoppingCart, Protos.ShoppingCartModel>().ReverseMap();
            CreateMap<Models.ShoppingCartItem, Protos.ShoppingCartItemModel>().ReverseMap();
        }
    }
}
