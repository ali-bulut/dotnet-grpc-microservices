using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;

namespace ShoppingCartGrpc.Services
{
    public class ShoppingCartService : ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly IMapper _mapper;
        private readonly DiscountService _discountService;

        public ShoppingCartService(ShoppingCartContext shoppingCartContext, ILogger<ShoppingCartService> logger, IMapper mapper, DiscountService discountService)
        {
            _shoppingCartContext = shoppingCartContext;
            _logger = logger;
            _mapper = mapper;
            _discountService = discountService;
        }

        public override async Task<ShoppingCartModel> GetShoppingCart(GetShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext.ShoppingCarts.FirstOrDefaultAsync(x => x.Username == request.Username);

            if(shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with Username={request.Username} is not found!"));
            }

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
            return shoppingCartModel;
        }

        public override async Task<ShoppingCartModel> CreateShoppingCart(ShoppingCartModel request, ServerCallContext context)
        {
            var shoppingCart = _mapper.Map<ShoppingCart>(request);

            if(await _shoppingCartContext.ShoppingCarts.AnyAsync(x => x.Username == shoppingCart.Username))
            {
                _logger.LogError($"Invalid Username for ShoppingCart creation. ShoppingCart with Username={shoppingCart.Username} already exists!");
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"ShoppingCart with Username={shoppingCart.Username} already exists.!"));
            }

            _shoppingCartContext.ShoppingCarts.Add(shoppingCart);
            await _shoppingCartContext.SaveChangesAsync();

            _logger.LogInformation($"ShoppingCart is successfully created for Username={shoppingCart.Username}.");

            return _mapper.Map<ShoppingCartModel>(shoppingCart);
        }

        public override async Task<RemoveItemFromShoppingCartResponse> RemoveItemFromShoppingCart(RemoveItemFromShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext.ShoppingCarts.FirstOrDefaultAsync(x => x.Username == request.Username);
            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with Username={request.Username} is not found!"));
            }

            var removedCartItem = shoppingCart.Items.FirstOrDefault(x => x.ProductId == request.RemovedCartItem.ProductId);
            if(removedCartItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"CartItem with ProductId={request.RemovedCartItem.ProductId} is not found!"));
            }

            shoppingCart.Items.Remove(removedCartItem);
            var removedCount = await _shoppingCartContext.SaveChangesAsync();

            return new RemoveItemFromShoppingCartResponse { Success = removedCount > 0 };
        }

        public override async Task<AddItemIntoShoppingCartResponse> AddItemIntoShoppingCart(IAsyncStreamReader<AddItemIntoShoppingCartRequest> requestStream, ServerCallContext context)
        {
            while(await requestStream.MoveNext())
            {
                var shoppingCart = await _shoppingCartContext.ShoppingCarts.FirstOrDefaultAsync(x => x.Username == requestStream.Current.Username);
                if (shoppingCart == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with Username={requestStream.Current.Username} is not found!"));
                }

                var addedCartItem = _mapper.Map<ShoppingCartItem>(requestStream.Current.NewCartItem);

                var cartItem = shoppingCart.Items.FirstOrDefault(x => x.ProductId == addedCartItem.ProductId);
                if(cartItem != null)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    var discount = await _discountService.GetDiscount(requestStream.Current.DiscountCode);
                    addedCartItem.Price -= discount.Amount;

                    shoppingCart.Items.Add(addedCartItem);
                }
            }

            var insertCount = await _shoppingCartContext.SaveChangesAsync();

            return new AddItemIntoShoppingCartResponse { Success = insertCount > 0, InsertCount = insertCount };
        }
    }
}
