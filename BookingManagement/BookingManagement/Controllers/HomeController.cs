using BookingManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookingManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookingSystemContext _context;

        public HomeController(ILogger<HomeController> logger, BookingSystemContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string? searchTerm)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;

          /*  IQueryable<Provider> providersQuery = _context.Providers
        .Where(p => _context.Availabilities.Any(a => a.ProviderId == p.Id &&
                                                     a.AvailableSlots > 0 &&
                                                     !(a.IsBlocked ?? false)));*/ // Ensure availability


            IQueryable<Provider> providersQuery = _context.Providers
       .Where(p => _context.Availabilities.Any(a => a.ProviderId == p.Id &&
                                                    a.AvailableSlots > 0 )); // Ensure availability

            if (categoryId.HasValue)
            {
                providersQuery = providersQuery.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                providersQuery = providersQuery.Where(p => p.Name.Contains(searchTerm));
            }

            var providersWithReviews = await providersQuery
     .Select(p => new ProviderWithReviewsViewModel
     {
         Provider = p,
         LatestReview = _context.Reviews
             .Where(r => r.ProviderId == p.Id)
             .OrderByDescending(r => r.Id) // Get the latest review
             .Select(r => new ReviewViewModel
             {
                 ReviewText = r.Review1,
                 Rating = r.Rating ?? 0, // Prevent null ratings
                 UserName = _context.Users
                     .Where(u => u.Id == r.UserId)
                     .Select(u => u.FullName)
                     .FirstOrDefault() ?? "Unknown User"
             })
             .FirstOrDefault()
     })
     .ToListAsync();

            return View(providersWithReviews);
        }

        public async Task<IActionResult> Explore()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }


        public async Task<IActionResult> GetReviews(int providerId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProviderId == providerId)
                .OrderByDescending(r => r.Id) // Latest first
                .Select(r => new
                {
                    reviewText = r.Review1,
                    rating = r.Rating ?? 0, // Default to 0 if null
                    userName = _context.Users.Where(u => u.Id == r.UserId).Select(u => u.FullName).FirstOrDefault() ?? "Unknown User"
                })
                .ToListAsync();

            return Json(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReview([FromBody] ReviewSaveModel model)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != null && userRole != "User")
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            if (userId == 0)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            var review = new Review
            {
                UserId = userId,
                ProviderId = model.ProviderId,
                Review1 = model.ReviewText,
                Rating = model.Rating
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public async Task<IActionResult> List(int? categoryId)
        {
            IQueryable<Provider> providersQuery = _context.Providers
       .Where(p => _context.Availabilities.Any(a => a.ProviderId == p.Id &&
                                                    a.AvailableSlots > 0));

            if (categoryId.HasValue)
            {
                providersQuery = providersQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            var providersWithReviews = await providersQuery
             .Select(p => new ProviderWithReviewsViewModel
             {
                 Provider = p,
                 LatestReview = _context.Reviews
                     .Where(r => r.ProviderId == p.Id)
                     .OrderByDescending(r => r.Id) // Get the latest review
                     .Select(r => new ReviewViewModel
                     {
                         ReviewText = r.Review1,
                         Rating = r.Rating ?? 0, // Prevent null ratings
                         UserName = _context.Users
                             .Where(u => u.Id == r.UserId)
                             .Select(u => u.FullName)
                             .FirstOrDefault() ?? "Unknown User"
                     })
                     .FirstOrDefault()
             })
             .ToListAsync();

            return View(providersWithReviews);
        }



        public IActionResult Privacy()
        {
            return View();
        }public IActionResult AboutUs()
        {
            return View();
        }public IActionResult ContactUs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult Reserve(int providerId)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            //var userrule = HttpContent.Session.GetString("UserRole");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != null && userRole != "User")
            {
                return RedirectToAction("Index");

            }

            if (userId == 0)
            {
                return RedirectToAction("Login", "Users");
            }

            var provider = _context.Providers.FirstOrDefault(p => p.Id == providerId);
           
            if (provider == null)
            {
                return NotFound();
            }
            //var providerUser = _context.Users.FindAsync(provider.UserId);

            var availability = _context.Availabilities
            .Where(a => a.ProviderId == providerId && a.IsClosed == false)
            .ToList();

            var model = new ReservationViewModel
            {
                ProviderId = providerId,
                SenderId= userId,
                ProviderUserId = _context.Providers.Find(providerId)?.UserId ??0,
                ProviderName = _context.Providers.Find(providerId)?.Name ?? "Unknown",
                Website= _context.Providers.Find(providerId)?.Website ?? "Unknown",
                Address = _context.Providers.Find(providerId)?.Address ?? "Unknown",

                AvailableDates = availability.Select(a => a.Date.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd")).ToList(),
                
            };

            Console.WriteLine("Available Dates: " + string.Join(", ", model.Website));

            return View("Reservation", model);
        }





        [HttpPost]
        public async Task<IActionResult> MakeReservation(ReservationRequestModel model)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != null && userRole != "User")
            {
                return RedirectToAction("Index");
            }

            if (userId == 0)
            {
                return RedirectToAction("Login", "Users");
            }

            // Combine SelectedDate (DateOnly) and SelectedTime (TimeOnly) into a DateTime
            DateTime bookingDateTime = model.SelectedDate.ToDateTime(model.SelectedTime);


            // Fetch provider details
            var provider = await _context.Providers.FindAsync(model.ProviderId);
           

            // Fetch provider's user
            var providerUser = await _context.Users.FindAsync(provider.UserId);
            if (providerUser != null && providerUser.Requests > 0)
            {
                providerUser.Requests -= 1; // Decrement request count
                _context.Users.Update(providerUser);
            }


            var booking = new Booking
            {
                UserId = userId,
                ProviderId = model.ProviderId,
                BookingDate = bookingDateTime,
                Status = "Pending",
                Notes = model.Notes,
                CreatedAt = DateTime.Now,
                Nights=model.Nights,
                People=model.People
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Fetch User Email
            var user = await _context.Users.FindAsync(userId);
/*            if (user != null)
            {
                var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();
                string subject = "Booking Confirmation";
                string body = $@"
            <p>Hello {user.FullName},</p>
            <p>Your booking for <strong>{bookingDateTime}</strong> has been received and is pending confirmation.</p>
            <p>Details:</p>
            <ul>
                <li><strong>Service Provider:</strong> Hotel</li>
                <li><strong>Notes:</strong> {model.Notes}</li>
            </ul>
            <p>Thank you for using our service!</p>";

                await emailService.SendEmailAsync(user.Email, subject, body);
            }*/

            return RedirectToAction("Index", "Home"); // Redirect to home or confirmation page
        }

        [HttpPost]
        public async Task<IActionResult> SendChatMessage([FromBody] ChatMessage chat)
        {
            chat.SentAt = DateTime.Now;

            _context.ChatMessages.Add(chat);
            await _context.SaveChangesAsync();

            return Ok();
        }









    }
}
