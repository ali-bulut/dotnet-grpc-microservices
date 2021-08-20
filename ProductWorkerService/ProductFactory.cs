using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;

namespace ProductWorkerService
{
    public class ProductFactory
    {
        private readonly ILogger<ProductFactory> _logger;
        private readonly IConfiguration _configuration;

        public ProductFactory(ILogger<ProductFactory> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task<AddProductRequest> Generate()
        {
            var productName = $"{_configuration.GetValue<string>("WorkerService:ProductName")}_{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()}";
            var productRequest = new AddProductRequest
            {
                Product = new ProductModel
                {
                    Name = productName,
                    Description = $"{productName}_WorkerServiceDescription",
                    Price = new Random().Next(1000),
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow),
                    Status = ProductStatus.Instock
                }
            };

            return Task.FromResult(productRequest);
        }
    }
}
