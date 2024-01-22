using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AiDesigner.Server.Data;
using AiDesigner.Shared.Blocks;
using AiDesigner.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodeBaseApi.Version2;
using Stripe.Checkout;

namespace NodeBaseApi.StripeController
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class StripeController : ControllerBase
    {
        [HttpPost("create-stripe-session")]
        public async Task<IActionResult> CreateStripeSession()
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = 2000, // price in cents
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "T-shirt",
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
        public async Task<IActionResult> CreateSubscriptionSession()
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                Price = "price_YOUR_SUBSCRIPTION_PRICE_ID", // Replace with your Stripe price ID
                Quantity = 1,
            },
        },
                Mode = "subscription",
                SuccessUrl = "https://example.com/success", // Replace with your success URL
                CancelUrl = "https://example.com/cancel", // Replace with your cancel URL
            };

            var service = new SessionService();
            Stripe.Checkout.Session session = await service.CreateAsync(options);

            return Ok(new { SessionId = session.Id });
        }
    }
}
