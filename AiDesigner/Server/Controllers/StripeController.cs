using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace NodeBaseApi.StripeController
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class StripeController : ControllerBase
    {
        [HttpPost("create-stripe-session")]
        public async Task<IActionResult> CreateStripeSession([FromBody] TokenRequest request)
        {
            int totalPrice = CalculatePrice(request.TokenAmount);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = totalPrice,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Token Purchase",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel",
            };

            var service = new SessionService();
            Stripe.Checkout.Session session = await service.CreateAsync(options);

            return Ok(new { SessionId = session.Id });
        }

        [HttpPost("create-subscription-session")]
        public async Task<IActionResult> CreateSubscriptionSession([FromBody] TokenRequest request)
        {
            int totalPrice = CalculateSubPrice(request.TokenAmount);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = $"price_{totalPrice}", // Replace with your Stripe price ID
                        Quantity = 1,
                    },
                },
                Mode = "subscription",
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel",
            };

            var service = new SessionService();
            Stripe.Checkout.Session session = await service.CreateAsync(options);

            return Ok(new { SessionId = session.Id });
        }

        private int CalculatePrice(int tokenAmount)
        {
            switch (tokenAmount)
            {
                case 24000:
                    return 599; // price in cents for $5.99
                case 40000:
                    return 999; // price in cents for $9.99
                case 80000:
                    return 1999; // price in cents for $19.99
                default:
                    throw new ArgumentException("Invalid token amount");
            }
        }

        private int CalculateSubPrice(int tokenAmount)
        {
            switch (tokenAmount)
            {
                case 2000:
                    return 999; // price in cents for $9.99
                case 8000:
                    return 2999; // price in cents for $29.99
                default:
                    throw new ArgumentException("Invalid token amount");
            }
        }
    }

    public class TokenRequest
    {
        public int TokenAmount { get; set; }
    }
}
