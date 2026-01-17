using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mess_Management_System.Services
{
    public class StripeService
    {
        private readonly IConfiguration _configuration;

        public StripeService(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        /// <summary>
        /// Creates a Stripe Checkout Session for bill payment
        /// </summary>
        public async Task<Session> CreateCheckoutSession(int billId, decimal amount, string userEmail, string userName, string successUrl, string cancelUrl)
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
                            Currency = "pkr", // Pakistani Rupee
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Bill Payment - Invoice #{billId}",
                                Description = $"Monthly mess bill for {userName}",
                            },
                            UnitAmount = (long)(amount * 100), // Stripe expects amount in cents
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                CustomerEmail = userEmail,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "bill_id", billId.ToString() },
                    { "user_email", userEmail }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session;
        }

        /// <summary>
        /// Retrieves a Checkout Session by ID
        /// </summary>
        public async Task<Session> GetSession(string sessionId)
        {
            var service = new SessionService();
            return await service.GetAsync(sessionId);
        }

        /// <summary>
        /// Verifies webhook signature for security
        /// </summary>
        public Event ConstructWebhookEvent(string json, string stripeSignature)
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            return EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
        }
    }
}