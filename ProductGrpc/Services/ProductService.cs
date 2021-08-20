using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Protos;

namespace ProductGrpc.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {
        private readonly ILogger<ProductService> _logger;
        private readonly ProductContext _productContext;

        public ProductService(ILogger<ProductService> logger, ProductContext productContext)
        {
            _logger = logger;
            _productContext = productContext;
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            return base.Test(request, context);
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            var product = await _productContext.Products.FindAsync(request.ProductId);
            if(product == null)
            {
                // throw an rpc exception
            }

            var productModel = new ProductModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Status = ProductStatus.Instock,
                CreatedTime = Timestamp.FromDateTime(product.CreatedTime)
            };

            return productModel;
        }

        public override async Task GetAllProducts(GetAllProductsRequest request, IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            var productList = await _productContext.Products.ToListAsync();

            foreach (var product in productList)
            {
                var productModel = new ProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(product.CreatedTime)
                };

                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var product = new Models.Product
            {
                Id = request.Product.Id,
                Name = request.Product.Name,
                Description = request.Product.Description,
                Price = request.Product.Price,
                Status = Models.ProductStatus.INSTOCK,
                CreatedTime = request.Product.CreatedTime.ToDateTime()
            };

            await _productContext.Products.AddAsync(product);

            await _productContext.SaveChangesAsync();

            var productModel = new ProductModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Status = Protos.ProductStatus.Instock,
                CreatedTime = Timestamp.FromDateTime(product.CreatedTime)
            };

            return productModel;
        }
    }
}
