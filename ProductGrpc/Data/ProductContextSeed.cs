using System;
using System.Collections.Generic;
using System.Linq;
using ProductGrpc.Models;

namespace ProductGrpc.Data
{
    public class ProductContextSeed
    {
        public static void SeedAsync(ProductContext productContext)
        {
            if (!productContext.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        Name = "iPhone XS",
                        Description = "Brand new iPhone device",
                        Price = 699,
                        Status = ProductStatus.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Macbook Pro M1 13 inch",
                        Description = "Macbook Pro with brand new M1 processor",
                        Price = 1299,
                        Status = ProductStatus.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = 3,
                        Name = "Airpods Pro",
                        Description = "Apple headphones with Active Noise Cancellation",
                        Price = 199,
                        Status = ProductStatus.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                };
                productContext.Products.AddRange(products);
                productContext.SaveChanges();
            }
        }
    }
}
