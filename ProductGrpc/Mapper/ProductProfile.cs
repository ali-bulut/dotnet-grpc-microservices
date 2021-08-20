using System;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace ProductGrpc.Mapper
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Models.Product, Protos.ProductModel>()
                .ForMember(dest => dest.CreatedTime,
                opt => opt.MapFrom(src => Timestamp.FromDateTime(src.CreatedTime)));

            CreateMap<Protos.ProductModel, Models.Product>()
                .ForMember(dest => dest.CreatedTime,
                opt => opt.MapFrom(src => src.CreatedTime.ToDateTime()));
        }
    }
}
