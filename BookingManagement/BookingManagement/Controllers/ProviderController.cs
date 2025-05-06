using Microsoft.AspNetCore.Mvc;
using BookingManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Windows;
using System.Net;

namespace BookingManagement.Controllers
{
    public class ProviderController : Controller
    {
        private readonly BookingSystemContext _context;

        public ProviderController(BookingSystemContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        // GET: Provider Dashboard
        public async Task<IActionResult> Dashboard(int skip = 0)
        {
            // ✅ Get logged-in user's ID
            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            // ✅ Check if the user already has a provider record
            var providers = await _context.Providers.Where(p => p.UserId == userId).ToListAsync();

            // 🔥 If no provider exists, create one automatically
            if (!providers.Any())
            {
                var newProvider = new Provider
                {
                    UserId = userId,
                    Name = "New Provider", // You can update this dynamically
                    Address = "ABC",
                    Phone = "12345",

                    CategoryId = 1, // Set category if needed
                    CreatedAt = DateTime.UtcNow
                };

                _context.Providers.Add(newProvider);
                await _context.SaveChangesAsync();

                // ✅ Fetch the newly created provider
                providers = new List<Provider> { newProvider };
            }

            // ✅ Extract provider IDs
            var providerIds = providers.Select(p => p.Id).ToList();

            // ✅ Get bookings for all providers
            var bookings = await _context.Bookings
                .Where(b => providerIds.Contains(b.ProviderId))
                .Include(b => b.User)
                .ToListAsync();

            // ✅ Get services linked to the user
            var services = await _context.Providers
                .Where(p => p.UserId == userId)
                .Include(p => p.Category)
                .ToListAsync();

            // ✅ Get availability for all providers
            var availability = await _context.Availabilities
                .Where(a => providerIds.Contains(a.ProviderId))
                .ToListAsync();

            // ✅ Fetch Available Requests for this user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            int availableRequests = user?.Requests ?? 0;

            var chatMessages = await _context.ChatMessages
            .Include(c => c.Sender)
            .Include(c => c.Receiver)
            .Where(c => c.SenderId == userId || c.ReceiverId == userId)
            .OrderBy(c => c.SentAt)
            .ToListAsync();

            ViewBag.ProviderId = userId;


            // ✅ Pass all data to the view as a tuple
            return View(Tuple.Create(bookings, services, availability, availableRequests, chatMessages, userId));
        }




        // POST: Update Booking Status
        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, string status)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Dashboard");
        }

        // GET: Edit Provider Profile
        public async Task<IActionResult> EditProfile()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null) return NotFound();
            return View(provider);
        }

        // POST: Save Provider Profile
        [HttpPost]
        public async Task<IActionResult> EditProfile(Provider provider, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var dbProvider = await _context.Providers.FindAsync(provider.Id);
                if (dbProvider == null) return NotFound();

                dbProvider.Name = provider.Name;
                dbProvider.Address = provider.Address;
                dbProvider.Description = provider.Description;
                dbProvider.Website = provider.Website;
                dbProvider.Phone = provider.Phone;
                dbProvider.Status = provider.Status;
                dbProvider.CategoryId = provider.CategoryId;

                if (imageFile != null)
                {
                    using var ms = new MemoryStream();
                    await imageFile.CopyToAsync(ms);
                    dbProvider.Image = ms.ToArray();
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard");
            }
            return View(provider);
        }

        // GET: Manage Availability
        public async Task<IActionResult> ManageAvailability()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0"); // ✅ Get logged-in user ID
            Console.WriteLine($"User ID: {userId}"); // Debug log

            // ✅ Get all availability records where UserId matches the logged-in user
            var availability = await _context.Availabilities
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Date)
                .ToListAsync();

            // ✅ Get only providers related to this user
            var providers = await _context.Providers
                .Where(p => p.UserId == userId)
                .ToListAsync();

            // ✅ Debugging: Check if correct records are being fetched
            Console.WriteLine($"Fetched {availability.Count} availability records for User ID: {userId}");

            foreach (var a in availability)
            {
                Console.WriteLine($"Availability: ProviderId={a.ProviderId}, Date={a.Date}, Slots={a.AvailableSlots}");
            }

            return View(Tuple.Create(new List<Booking>(), providers, availability));
        }





        // POST: Update Availability
        [HttpPost]
        public async Task<IActionResult> UpdateAvailability(int providerId, DateOnly date, int slots)
        {
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.ProviderId == providerId && a.Date == date);

            if (availability != null)
            {
                availability.AvailableSlots = slots;
            }
            else
            {
                _context.Availabilities.Add(new Availability
                {
                    ProviderId = providerId,
                    Date = date,
                    AvailableSlots = slots
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageAvailability");
        }

        [HttpPost]
        public async Task<IActionResult> SetAvailability(int providerId, DateOnly date, DateOnly toDate, int availableSlots,
                                         TimeOnly? openTime, TimeOnly? closeTime,
                                         bool isClosed, bool isBlocked)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            for (DateOnly currentDate = date; currentDate <= toDate; currentDate = currentDate.AddDays(1))
            {
                var existingAvailability = await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.ProviderId == providerId && a.Date == currentDate);

                if (existingAvailability == null)
                {
                    var newAvailability = new Availability
                    {
                        ProviderId = providerId,
                        UserId = userId,
                        Date = currentDate,
                        AvailableSlots = availableSlots,
                        OpenTime = openTime,
                        CloseTime = closeTime,
                        IsClosed = isClosed,
                        IsBlocked = isBlocked
                    };

                    _context.Availabilities.Add(newAvailability);
                }
                else
                {
                    existingAvailability.AvailableSlots = availableSlots;
                    existingAvailability.OpenTime = openTime;
                    existingAvailability.CloseTime = closeTime;
                    existingAvailability.IsClosed = isClosed;
                    existingAvailability.IsBlocked = isBlocked;

                    _context.Availabilities.Update(existingAvailability);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");

        }



        [HttpPost]
        public async Task<IActionResult> DeleteAvailability(int availabilityId)
        {
            var availability = await _context.Availabilities.FindAsync(availabilityId);
            if (availability != null)
            {
                _context.Availabilities.Remove(availability);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> EditService(int? id)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            var provider = id == null ? new Provider { UserId = userId } :
                await _context.Providers.FindAsync(id);

            if (provider == null || provider.UserId != userId)
                return RedirectToAction("Dashboard");

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(provider);
        }

        [HttpPost]
        public async Task<IActionResult> SaveService(Provider provider, IFormFile? ImageFile)
        {
            // Get UserId from Session
            provider.UserId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            /*if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View("EditService", provider);
            }*/


            // **Check if provider.Id is valid**
            if (provider.Id > 0)
            {
                var existingProvider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == provider.Id && p.UserId == provider.UserId);

                if (existingProvider != null)
                {
                    // **Update Existing Provider**
                    existingProvider.Name = provider.Name;
                    existingProvider.Price = provider.Price;
                    existingProvider.Address = provider.Address;
                    existingProvider.Description = provider.Description;
                    existingProvider.Website = provider.Website;
                    existingProvider.Phone = provider.Phone;
                    existingProvider.MaxBookingsPerDay = provider.MaxBookingsPerDay;
                    existingProvider.Status = provider.Status;
                    existingProvider.CategoryId = provider.CategoryId;

                    if (ImageFile != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await ImageFile.CopyToAsync(ms);
                            existingProvider.Image = ms.ToArray();
                        }
                    }

                    _context.Providers.Update(existingProvider);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Dashboard");
                }
            }

            // **If provider.Id is 0, create a new provider**
            if (ImageFile != null)
            {
                using (var ms = new MemoryStream())
                {
                    await ImageFile.CopyToAsync(ms);
                    provider.Image = ms.ToArray();
                }
            }

            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }




        [HttpPost]
        public async Task<IActionResult> UpdateRequests([FromBody] UpdateRequestModel model)
        {
            Console.WriteLine($"Received: Requests = {model.Requests}, Amount = {model.Amount}");

            if (model == null || model.Requests <= 0 || model.Amount <= 0)
            {
                return BadRequest(new { message = "Invalid request amount." });
            }

            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            // Add payment record with the actual amount sent from the frontend
            var payment = new Payment
            {

                UserId = userId,
                Amount = model.Amount, // Now using the actual amount paid
                PaymentMethod = "Paypal"
            };
            _context.Payments.Add(payment);

            var user = await _context.Users.FindAsync(userId);

            //Console.WriteLine($"Received: Requests = {model.Requests}, Amount = {userId}");

            if (user == null)
            {
                return Unauthorized(new { message = "User not found." });
            }


            // Check if the user has a referral signup code
            var sign = user.SignupCode;

            if (!string.IsNullOrEmpty(sign))
            {
                // Find the affiliate using the referral code
                var affiliate = await _context.Affiliates.FirstOrDefaultAsync(a => a.ReferralCode == sign);

                if (affiliate != null)
                {
                    // Add 10% of the payment amount to the affiliate's balance
                    decimal commission = model.Amount * 0.10m;
                    affiliate.UserCredit += commission;
                    _context.Affiliates.Update(affiliate);
                }
            }



            user.Requests += model.Requests;
            _context.Users.Update(user);

          

          

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Successfully added {model.Requests} requests!" });
        }

        // Model for receiving request data
        public class UpdateRequestModel
        {
            public int Requests { get; set; }
            public decimal Amount { get; set; }
        }





        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            // Find the booking
            var booking = await _context.Bookings
                .Include(b => b.User) // Include User details if needed
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("Dashboard");
            }

            // Ensure the status is valid
            if (status != "Confirmed" && status != "Cancelled")
            {
                TempData["Error"] = "Invalid status update.";
                return RedirectToAction("Dashboard");
            }

            // Update status
            booking.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Booking status changed to {status}.";
            return RedirectToAction("Dashboard"); // Redirect to booking management page
        }


    }
}
