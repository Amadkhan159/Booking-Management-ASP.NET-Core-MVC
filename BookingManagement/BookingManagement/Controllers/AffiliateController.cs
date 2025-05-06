using Microsoft.AspNetCore.Mvc;
using BookingManagement.Models;
using BookingManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace BookingManagement.Controllers
{
    public class AffiliateController : Controller
    {
        private readonly BookingSystemContext _context;

        public AffiliateController(BookingSystemContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            if (userId == 0)
            {
                return RedirectToAction("Login", "Users");
            }

            // Find the affiliate account linked to the user
            var affiliate = _context.Affiliates.FirstOrDefault(a => a.UserId == userId);

            if (affiliate == null)
            {
                // Generate a random referral code (9 characters with letters and numbers)
                string referralCode = GenerateReferralCode();

                // Create a new affiliate record
                affiliate = new Affiliate
                {
                    UserId = userId,
                    ReferralCode = referralCode,
                    CommissionRate = 5.0m, // Default 5% commission
                    CreatedAt = DateTime.UtcNow,
                    UserCredit = 0.00m
                };

                _context.Affiliates.Add(affiliate);
                _context.SaveChanges();
            }

            // Find users who signed up using the affiliate's referral code
            var referredUsers = _context.Users
                .Where(u => u.SignupCode == affiliate.ReferralCode)
                .ToList(); // Keep it as List<User>

            // Store referred user IDs
            var referredUserIds = referredUsers.Select(u => u.Id).ToList();

            // Fetch payment history for referred users
            var payments = _context.Payments
                .Where(p => referredUserIds.Contains(p.UserId))
                .ToList(); // Fetch into memory first

            var paymentHistory = payments
                .Select(p => new PaymentHistoryViewModel
                {
                    FullName = referredUsers.FirstOrDefault(u => u.Id == p.UserId) != null
                               ? referredUsers.First(u => u.Id == p.UserId).FullName
                               : "Unknown",
                    AmountPaid = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    DatePaid = p.TransactionDate
                })
                .ToList();

            var model = new AffiliateDashboardViewModel
            {
                ReferralCode = affiliate.ReferralCode,
                UserCredit = affiliate.UserCredit ?? 0,
                ReferredUsers = referredUsers.Select(u => new ReferredUserViewModel
                {
                    FullName = u.FullName,
                    Phone = u.Phone
                }).ToList(),
                PaymentHistory = paymentHistory
            };

            return View(model);
        }

        private string GenerateReferralCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 9)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        [HttpPost]
        public async Task<IActionResult> CreatePayPalPayout()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Unauthorized();

            var affiliate = await _context.Affiliates.FirstOrDefaultAsync(a => a.UserId == int.Parse(userId));
            if (affiliate == null || affiliate.Balance <= 0)
                return BadRequest(new { success = false, message = "Insufficient balance." });

            // PayPal Payout API URL
            string url = "https://api-m.sandbox.paypal.com/v1/payments/payouts";
            string accessToken = await GetPayPalAccessToken(); // Get access token

            var payoutData = new
            {
                sender_batch_header = new { email_subject = "Affiliate Withdrawal" },
                items = new[]
                {
            new
            {
                recipient_type = "EMAIL",
                amount = new { value = affiliate.Balance, currency = "USD" },
                receiver = affiliate.PayPalEmail, // User's PayPal email
                note = "Affiliate earnings withdrawal"
            }
        }
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await client.PostAsJsonAsync(url, payoutData);
                if (!response.IsSuccessStatusCode) return BadRequest(new { success = false, message = "PayPal transaction failed." });

                // Set balance to zero after successful withdrawal
                affiliate.Balance = 0;
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
        }

        // **Helper Method: Get PayPal Access Token**
        private async Task<string> GetPayPalAccessToken()
        {
            string clientId = "YOUR_PAYPAL_CLIENT_ID";
            string secret = "YOUR_PAYPAL_SECRET";
            string url = "https://api-m.sandbox.paypal.com/v1/oauth2/token";

            using (var client = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes($"{clientId}:{secret}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" }
        });

                var response = await client.PostAsync(url, content);
                var responseData = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseData);
                return json.GetProperty("access_token").GetString();
            }
        }


    }
}
