using System;
using System.Collections.Generic;
using System.Linq;
using ShoppingCartGrpc.Models;

namespace ShoppingCartGrpc.Data
{
    public class ShoppingCartContextSeed
    {
        public static void SeedAsync(ShoppingCartContext shoppingCartContext)
        {
            if (!shoppingCartContext.ShoppingCarts.Any())
            {
                var shoppingCarts = new List<ShoppingCart>
                {
                    new ShoppingCart
                    {
                        Username = "swn",
                        Items = new List<ShoppingCartItem>
                        {
                            new ShoppingCartItem
                            {
                                ProductId = 2,
                                ProductName = "Macbook Pro M1 13 inch",
                                Price = 1299,
                                Quantity = 2,
                                Color = "Black"
                            },
                            new ShoppingCartItem
                            {
                                ProductId = 3,
                                ProductName = "Airpods Pro",
                                Price = 199,
                                Quantity = 1,
                                Color = "White"
                            }
                        }
                    },
                };
                shoppingCartContext.ShoppingCarts.AddRange(shoppingCarts);
                shoppingCartContext.SaveChanges();
            }
        }
    }
}
