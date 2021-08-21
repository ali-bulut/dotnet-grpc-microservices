using System;
using System.Collections.Generic;

namespace ShoppingCartGrpc.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();

        public ShoppingCart()
        {

        }

        public ShoppingCart(string username)
        {
            Username = username;
        }

        public float TotalPrice
        {
            get
            {
                float totalPrice = 0;
                foreach (var item in Items)
                {
                    totalPrice += item.Price * item.Quantity;
                }
                return totalPrice;
            }
        }
    }
}
