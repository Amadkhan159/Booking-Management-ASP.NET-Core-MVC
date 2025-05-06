using System;
using Microsoft.AspNetCore.Mvc;
using BookingManagement.Models;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Security.Cryptography;

namespace BookingManagement.Controllers
{
    public class ManagerController : Controller
    {
        private readonly BookingSystemContext _context;

        public ManagerController(BookingSystemContext context)
        {
            _context = context;
        }

        // GET: Manager
        [HttpGet]
        public IActionResult Dashboard()
        {
            var categories = _context.Categories.ToList();
            var users = _context.Users.ToList();
            return View(Tuple.Create(categories, users));
        }

        [HttpPost]
        public async Task<IActionResult> SaveCategory(int Id, string Name, IFormFile? Picture)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return Json(new { success = false });

            Category category;

            if (Id == 0) // Add new category
            {
                category = new Category { Name = Name };
                if (Picture != null)
                {
                    using var ms = new MemoryStream();
                    await Picture.CopyToAsync(ms);
                    category.Picture = ms.ToArray();
                }
                _context.Categories.Add(category);
            }
            else // Update existing category
            {
                category = _context.Categories.Find(Id);
                if (category == null) return Json(new { success = false });

                category.Name = Name;
                if (Picture != null)
                {
                    using var ms = new MemoryStream();
                    await Picture.CopyToAsync(ms);
                    category.Picture = ms.ToArray();
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }



        [HttpPost]
        public async Task<IActionResult> UpdateUser(int Id, string FullName, string Email, string Phone, string Status, string PasswordHash)
        {
            if (Id == 0)
            {
                return Json(new { success = false, error = "Invalid user data." });
            }

            var user = await _context.Users.FindAsync(Id);
            if (user == null)
            {
                return Json(new { success = false, error = "User not found." });
            }

            // Update the user properties
            user.FullName = FullName;
            user.Email = Email;
            user.Phone = Phone;
            user.Status = Status;

            // Update password if provided (you can add more logic here if needed)
            if (!string.IsNullOrWhiteSpace(PasswordHash) && PasswordHash != "********")
            {
                //var hasher = new PasswordHasher<User>();
                user.PasswordHash = HashPassword( PasswordHash);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
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
