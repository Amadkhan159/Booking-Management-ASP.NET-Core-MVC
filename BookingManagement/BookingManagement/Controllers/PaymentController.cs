using BookingManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Controllers
{
    public class PaymentController : Controller
    {
        private readonly BookingSystemContext _context;

        public PaymentController(BookingSystemContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStripeSession([FromBody] PaymentRequest model)
        {
            StripeConfiguration.ApiKey = "sk_test_your_secret_key_here"; // Replace with your Secret Key

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"{model.Requests} Requests"
                            },
                            UnitAmount = model.Price * 100 // Stripe uses cents
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Payment/Success?requests={model.Requests}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Payment/Cancel"
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return Json(new { sessionUrl = session.Url });
        }

        [HttpGet]
        public async Task<IActionResult> Success(int requests)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                user.Requests += requests;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard", "Provider");
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            return RedirectToAction("Dashboard", "Provider");
        }

        public class PaymentRequest
        {
            public int Price { get; set; }
            public int Requests { get; set; }
        }
    }
}
