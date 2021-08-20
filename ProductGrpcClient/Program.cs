using System;
using System.Threading.Tasks;
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

            Console.ReadKey();
        }

        private static async Task GetAllProductsAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("GetAllProductsAsync Started...");

            // old version
            //using(var clientData = client.GetAllProducts(new GetAllProductsRequest()))
            //{
            //    while (await clientData.ResponseStream.MoveNext(new System.Threading.CancellationToken()))  
            //    {
            //        var currentProduct = clientData.ResponseStream.Current;
            //        Console.WriteLine("GetAllProductsAsync Response: " + currentProduct.ToString());
            //    }
            //}

            // C# 9.0
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
