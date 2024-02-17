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
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], "whsec_FhbDyhoqpkHsWvBZHvjEtym20CY0yPv4");

                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
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
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while processing the payment success notification. Error: {ex.Message}");
            }
        }

        [HttpPost("stripe/subscription-updated")]
        public async Task<IActionResult> OnSubscriptionUpdated()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], "whsec_qTqoX61icim3gUzWq8oeWdMNb8UCHaWx");

                if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
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
            catch (Exception ex)
            {
                // Log the exception details here using your preferred logging framework
                // For example: _logger.LogError(ex, "An error occurred processing the Stripe webhook.");

                // Return a BadRequest with a generic error message or a specific one based on the exception.
                // For security reasons, avoid sending detailed exception messages in production environments.
                return BadRequest($"An error occurred while processing the request. Error: {ex.Message}");
            }
        }
    }
}
