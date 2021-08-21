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

                var scModel = await GetOrCreateShoppingCartAsync(scClient);

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
