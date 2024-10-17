using System.Collections.Generic;
using E_commerce.Configurations;
using E_commerce.Utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Razorpay.Api;

namespace E_commerce.Services
{
    public class RazorpayService : IRazorpayService
    {
        private readonly string _key;
        private readonly string _secret;

        public RazorpayService(IOptions<RazorpayConfig> razorpayConfig)
        {
            _key = Environment.GetEnvironmentVariable("RAZORPAY_KEY") ?? razorpayConfig.Value.Key;
            _secret = Environment.GetEnvironmentVariable("RAZORPAY_SECRET") ?? razorpayConfig.Value.Secret;

        }

        public async Task<Razorpay.Api.Order> CreateOrderAsync(decimal amount, string currency = "INR", string receipt = "order_rcptid_11")
        {
            var client = new RazorpayClient(_key, _secret);

            var options = new Dictionary<string, object>
            {
                { "amount", amount * 100 },
                { "currency", currency },
                { "receipt", receipt },
                { "payment_capture", 1 }
            };

            Razorpay.Api.Order order = client.Order.Create(options);

            string orderJson = JsonConvert.SerializeObject(order.Attributes, Formatting.Indented);

            return order;
        }
    }
}
