using System;
using System.Threading.Tasks;
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

            Console.WriteLine("GetProductAsync Started...");

            var response = await client.GetProductAsync(new GetProductRequest { ProductId = 1 });

            Console.WriteLine("GetProductAsync Response: " + response.ToString());
            Console.ReadKey();
        }
    }
}
