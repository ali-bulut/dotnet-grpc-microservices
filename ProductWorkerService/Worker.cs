using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;

namespace ProductWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using var channel = GrpcChannel.ForAddress(_configuration.GetValue<string>("WorkerService:ServerUrl"));
                var client = new ProductProtoService.ProductProtoServiceClient(channel);

                await AddProductAsync(client);

                var interval = _configuration.GetValue<int>("WorkerService:TaskInterval");
                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task AddProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("AddProductAsync Started..."); ;

            var addProductResponse = await client.AddProductAsync(new AddProductRequest
            {
                Product = new ProductModel
                {
                    Name = _configuration.GetValue<string>("WorkerService:ProductName") + DateTimeOffset.Now,
                    Description = "WorkerService Description",
                    Price = 10,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow),
                    Status = ProductStatus.Instock
                }
            });

            Console.WriteLine("AddProductAsync response: " + addProductResponse.ToString());
        }
    }
}
