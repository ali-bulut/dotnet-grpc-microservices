using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;

namespace ProductGrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            await GetProductAsync(client);
            await GetAllProductsAsync(client);
            await AddProductAsync(client);

            Console.ReadKey();
        }

        private static async Task AddProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("AddProductAsync Started..."); ;

            var addProductResponse = await client.AddProductAsync(new AddProductRequest
            {
                Product = new ProductModel
                {
                    Name = "Test Product",
                    Description = "Test Description",
                    Price = 10,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow),
                    Status = ProductStatus.Instock
                }
            });

            Console.WriteLine("AddProductAsync response: " + addProductResponse.ToString());
        }

        private static async Task GetAllProductsAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("GetAllProductsAsync Started...");

            using var clientData = client.GetAllProducts(new GetAllProductsRequest());
            await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine("GetAllProductsAsync Response: " + responseData.ToString());
            }
        }

        private static async Task GetProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("GetProductAsync Started...");
            var response = await client.GetProductAsync(new GetProductRequest { ProductId = 1 });
            Console.WriteLine("GetProductAsync Response: " + response.ToString());
        }
    }
}
