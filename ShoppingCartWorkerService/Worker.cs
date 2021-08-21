using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using ShoppingCartGrpc.Protos;

namespace ShoppingCartWorkerService
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

                using var scChannel = GrpcChannel.ForAddress(_configuration.GetValue<string>("WorkerService:ShoppingCartServerUrl"));
                var scClient = new ShoppingCartProtoService.ShoppingCartProtoServiceClient(scChannel);

                // 1- create shopping cart if not exist
                var scModel = await GetOrCreateShoppingCartAsync(scClient);

                // 2- open shopping cart client stream
                using var scClientStream = scClient.AddItemIntoShoppingCart();

                // 3- retrieve products from product grpc with server stream
                using var pChannel = GrpcChannel.ForAddress(_configuration.GetValue<string>("WorkerService:ProductServerUrl"));
                var pClient = new ProductProtoService.ProductProtoServiceClient(pChannel);

                _logger.LogInformation("GetAllProducts Started...");
                using var clientData = pClient.GetAllProducts(new GetAllProductsRequest());
                await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
                {
                    // 4- add items into shopping cart with client stream
                    _logger.LogInformation("GetAllProducts Stream Response: " + responseData.ToString());

                    var newScItem = new AddItemIntoShoppingCartRequest
                    {
                        Username = _configuration.GetValue<string>("WorkerService:Username"),
                        DiscountCode = "CODE_100",
                        NewCartItem = new ShoppingCartItemModel
                        {
                            ProductId = responseData.Id,
                            ProductName = responseData.Name,
                            Price = responseData.Price,
                            Color = "Black",
                            Quantity = 1
                        }
                    };

                    await scClientStream.RequestStream.WriteAsync(newScItem);
                    _logger.LogInformation("ShoppingCart Client Stream added new item: " + newScItem.ToString());
                }

                await scClientStream.RequestStream.CompleteAsync();

                var addItemIntoShoppingCartResponse = await scClientStream;
                _logger.LogInformation("AddItemIntoShoppingCart Client Stream Response: " + addItemIntoShoppingCartResponse.ToString());


                await Task.Delay(_configuration.GetValue<int>("WorkerService:TaskInterval"), stoppingToken);
            }
        }

        private async Task<ShoppingCartModel> GetOrCreateShoppingCartAsync(ShoppingCartProtoService.ShoppingCartProtoServiceClient scClient)
        {
            ShoppingCartModel shoppingCartModel;
            var username = _configuration.GetValue<string>("WorkerService:Username");

            try
            {
                _logger.LogInformation("GetShoppingCartAsync Started...");
                shoppingCartModel = await scClient.GetShoppingCartAsync(new GetShoppingCartRequest { Username = username });
                _logger.LogInformation("GetShoppingCartAsync Response: " + shoppingCartModel.ToString());
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.NotFound)
                {
                    _logger.LogInformation("CreateShoppingCartAsync Started...");
                    shoppingCartModel = await scClient.CreateShoppingCartAsync(new ShoppingCartModel { Username = username });
                    _logger.LogInformation("CreateShoppingCartAsync Response: " + shoppingCartModel.ToString());
                }
                else
                {
                    throw;
                }
            }

            return shoppingCartModel;
        }
    }
}
