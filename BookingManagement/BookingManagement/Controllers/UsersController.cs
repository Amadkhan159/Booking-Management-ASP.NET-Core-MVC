using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using BookingManagement.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BookingManagement.Controllers
{
    public class UsersController : Controller
    {
        private readonly BookingSystemContext _context;

        public UsersController(BookingSystemContext context)
        {
            _context = context;
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("", "Email already exists.");
                    return View(user);
                }

                user.PasswordHash = HashPassword(user.PasswordHash);
                user.CreatedAt = DateTime.Now;
                user.Status = "Approved";

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(user);
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            var hashedPassword = HashPassword(password);
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }
            
            if (user.Status != "Approved")
            {
                ModelState.AddModelError("", "It will not approved yet.");
                return View();
            }


            


            // Store user session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", user.FullName);

            // Redirect based on user role
            switch (user.Role.ToLower())
            {
                case "provider":
                    return RedirectToAction("Dashboard", "Provider");                
                case "admin":
                    return RedirectToAction("Dashboard", "Manager");
                case "affiliate":
                    return RedirectToAction("Dashboard", "Affiliate"); // Placeholder, implement later
                case "user":
                default:
                    return RedirectToAction("Dashboard");
            }
        }


        // Logout
        public IActionResult Logout()
        {
            

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

       

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var bookings = _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Provider)
                .ToList();

            var chatMessages = _context.ChatMessages
            .Include(c => c.Sender)
            .Include(c => c.Receiver)
            .Where(c => c.SenderId == userId || c.ReceiverId == userId)
            .OrderByDescending(c => c.SentAt)
            .ToList();

            ViewBag.ChatMessages = chatMessages;
            ViewBag.CurrentUserId = userId;


            return View(bookings);
        }




        // GET: Users/Register
        public IActionResult RegisterPartnerProvider()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterPartnerProvider(User user)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("", "Email already exists.");
                    return View(user);
                }

                user.PasswordHash = HashPassword(user.PasswordHash);
                user.CreatedAt = DateTime.Now;
                user.Status = "Pending";

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(user);
        }





        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
