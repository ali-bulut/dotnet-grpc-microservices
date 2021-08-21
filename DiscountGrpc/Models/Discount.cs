using System;
namespace DiscountGrpc.Models
{
    public class Discount
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int Amount { get; set; }
    }
}
