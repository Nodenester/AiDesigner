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

        [HttpPost("stripe/payment-success")]
        public async Task<IActionResult> OnPaymentSuccess()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], "REDACTED_WEBHOOK_SECRET");

            if (stripeEvent.Type == Events.CheckoutSessionAsyncPaymentSucceeded)
            {
                var stripeSession = stripeEvent.Data.Object as Stripe.Checkout.Session;
                var userId = Guid.Parse(stripeSession.Metadata["userId"]);

                int tokensToAdd;
                switch (stripeSession.AmountTotal) 
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
                        tokensToAdd = 0; 
                        break;
                }

                await _dbConnection.AddBoughtTokensAsync(userId, tokensToAdd);
            }

            return Ok();
        }

        [HttpPost("stripe/subscription-updated")]
        public async Task<IActionResult> OnSubscriptionUpdated()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], "REDACTED_WEBHOOK_SECRET");

            if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
            {
                var subscription = stripeEvent.Data.Object as Subscription;
                var userId = Guid.Parse(subscription.Metadata["userId"]);

                int subscriptionTier = subscription.Items.Data
                    .FirstOrDefault()?.Price?.Id switch
                {
                    "price_1ObX2bGS8y7NleDORuIlzPCO" => 1,
                    "price_1ObX2bGS8y7NleDOvy9Btv2z" => 2,
                    _ => 0
                };

                await _dbConnection.SetSubscriptionTierAsync(userId, subscriptionTier);
            }
            else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
            {
                var subscription = stripeEvent.Data.Object as Subscription;
                var userId = Guid.Parse(subscription.Metadata["userId"]);

                await _dbConnection.SetSubscriptionTierAsync(userId, 0);
            }
            else if (stripeEvent.Type == Events.CustomerSubscriptionUpdated)
            {
                var subscription = stripeEvent.Data.Object as Subscription;
                var userId = Guid.Parse(subscription.Metadata["userId"]);

                int subscriptionTier = subscription.Items.Data
                    .FirstOrDefault()?.Price?.Id switch
                {
                    "price_1ObX2bGS8y7NleDORuIlzPCO" => 1,
                    "price_1ObX2bGS8y7NleDOvy9Btv2z" => 2, 
                    _ => 0 
                };

                await _dbConnection.SetSubscriptionTierAsync(userId, subscriptionTier);
            }
            return Ok();
        }
    }
}
