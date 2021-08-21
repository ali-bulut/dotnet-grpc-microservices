using System;
using System.Linq;
using System.Threading.Tasks;
using DiscountGrpc.Data;
using DiscountGrpc.Protos;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DiscountGrpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly ILogger<DiscountService> _logger;

        public DiscountService(ILogger<DiscountService> logger)
        {
            _logger = logger;
        }

        public override Task<DiscountModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var discount = DiscountContext.Discounts.FirstOrDefault(x => x.Code == request.DiscountCode);

            if(discount == null)
            {
                _logger.LogError($"Discount code ({request.DiscountCode}) is not found!");
                throw new RpcException(new Status(StatusCode.NotFound, $"Discount code ({request.DiscountCode}) is not found!"));
            }

            _logger.LogInformation($"Discount is operated with the {discount.Code} and the amount is {discount.Amount}");

            return Task.FromResult(new DiscountModel
            {
                DiscountId = discount.Id,
                Code = discount.Code,
                Amount = discount.Amount
            });
        }
    }
}
