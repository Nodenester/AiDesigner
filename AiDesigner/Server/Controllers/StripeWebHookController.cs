using Microsoft.AspNetCore.Mvc;
using Stripe;
using AiDesigner.Server.Data;
using NodeBaseApi.Version2;

namespace AiDesigner.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StripeWebHookController : Controller
    {
        private readonly DBConnection _dbConnection;

        public StripeWebHookController(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }



        [HttpPost("payment-success")]
        public async Task<IActionResult> OnPaymentSuccess()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], "your_webhook_secret");

            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                var stripeSession = stripeEvent.Data.Object as Stripe.Checkout.Session;
                var userId = Guid.Parse(stripeSession.Metadata["userId"]);

                int tokensToAdd;
                switch (stripeSession.AmountTotal) // Amount in cents
                {
                    case 599:
                        tokensToAdd = 24000;
                        break;
                    case 999:
                        tokensToAdd = 40000;
                        break;
                    case 1999:
                        tokensToAdd = 80000;
                        break;
                    default:
                        tokensToAdd = 0; // Handle unexpected amount
                        break;
                }

                await _dbConnection.AddBoughtTokensAsync(userId, tokensToAdd);
            }

            return Ok();
        }

        [HttpPost("subscription-updated")]
        public async Task<IActionResult> OnSubscriptionUpdated()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], "your_webhook_secret");

            if (stripeEvent.Type == Events.CustomerSubscriptionUpdated)
            {
                var subscription = stripeEvent.Data.Object as Subscription;
                var userId = Guid.Parse(subscription.Metadata["userId"]);

                int subscriptionTier = subscription.Items.Data
                    .FirstOrDefault()?.Price?.Id switch
                {
                    "price_xxx" => 1, // Replace with your actual price ID for $9.99 tier
                    "price_yyy" => 2, // Replace with your actual price ID for $29.99 tier
                    _ => 0 // Handle unexpected price ID
                };

                await _dbConnection.SetSubscriptionTierAsync(userId, subscriptionTier);
            }

            return Ok();
        }
    }
}
