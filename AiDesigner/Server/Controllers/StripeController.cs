using System;
using System.Collections.Generic;
using System.Security.Claims;
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
        [HttpPost("create-payment-session")]
        public async Task<ActionResult> CreateStripeSession([FromBody] int TokenAmount)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int totalPrice = CalculatePrice(TokenAmount);

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
                SuccessUrl = "https://nodenestor.com/home",
                CancelUrl = "https://nodenestor.com/paymentoptions",

                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId }
                }
            };

            var service = new SessionService();
            Stripe.Checkout.Session session = await service.CreateAsync(options);

            return Ok(new { SessionId = session.Id, Url = session.Url });
        }

        [HttpPost("create-subscription-session")]
        public async Task<ActionResult> CreateSubscriptionSession([FromBody] int TokenAmount)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string stripePriceId = GetStripePriceIdForSubscription(TokenAmount);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = stripePriceId, // Use the Stripe price ID
                        Quantity = 1,
                    },
                },
                Mode = "subscription",
                SuccessUrl = "https://nodenestor.com/home",
                CancelUrl = "https://nodenestor.com/paymentoptions",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId }
                }
            };

            var service = new SessionService();
            Stripe.Checkout.Session session = await service.CreateAsync(options);

            return Ok(new { SessionId = session.Id, Url = session.Url });
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

        private string GetStripePriceIdForSubscription(int tokenAmount)
        {
            switch (tokenAmount)
            {
                case 2000:
                    return "price_1ObX2bGS8y7NleDORuIlzPCO"; // Replace with the Stripe price ID for $9.99
                case 8000:
                    return "price_1ObX2bGS8y7NleDOvy9Btv2z"; // Replace with the Stripe price ID for $29.99
                default:
                    throw new ArgumentException("Invalid token amount");
            }
        }
    }
}
