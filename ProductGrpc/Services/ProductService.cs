using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public ProductService(ILogger<ProductService> logger, ProductContext productContext, IMapper mapper)
        {
            _logger = logger;
            _productContext = productContext;
            _mapper = mapper;
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
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID={request.ProductId} is not found!"));
            }

            var productModel = _mapper.Map<ProductModel>(product);
            return productModel;
        }

        public override async Task GetAllProducts(GetAllProductsRequest request, IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            var productList = await _productContext.Products.ToListAsync();

            foreach (var product in productList)
            {
                var productModel = _mapper.Map<ProductModel>(product);
                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var product = _mapper.Map<Models.Product>(request.Product);

            await _productContext.Products.AddAsync(product);
            await _productContext.SaveChangesAsync();

            _logger.LogInformation($"Product sucessfully added! -> {product.Id}-{product.Name}");

            var productModel = _mapper.Map<ProductModel>(product);

            return productModel;
        }

        public override async Task<ProductModel> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            var product = _mapper.Map<Models.Product>(request.Product);
            
            if(!await _productContext.Products.AnyAsync(x => x.Id == product.Id))
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID={product.Id} is not found!"));
            }

            _productContext.Entry(product).State = EntityState.Modified;

            try
            {
                await _productContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            var productModel = _mapper.Map<ProductModel>(product);
            return productModel;
        }

        public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
        {
            var product = await _productContext.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID={request.ProductId} is not found!"));
            }

            _productContext.Products.Remove(product);
            var deletedCount = await _productContext.SaveChangesAsync();

            return new DeleteProductResponse { Success = deletedCount > 0 };
        }

        public override async Task<InsertBulkProductResponse> InsertBulkProduct(IAsyncStreamReader<ProductModel> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var product = _mapper.Map<Models.Product>(requestStream.Current);
                _productContext.Products.Add(product);
            }

            var insertCount = await _productContext.SaveChangesAsync();

            return new InsertBulkProductResponse { Success = insertCount > 0, InsertCount = insertCount };
        }
    }
}
