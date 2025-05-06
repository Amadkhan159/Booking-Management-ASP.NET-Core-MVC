using BookingManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookingManagement.Controllers
{
    public class ChatController : Controller
    {
        private readonly BookingSystemContext _context;

        public ChatController(BookingSystemContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SaveMessage([FromBody] ChatMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.Message))
            {
                return BadRequest("Invalid message.");
            }

            message.SentAt = DateTime.Now;
            _context.ChatMessages.Add(message);
            _context.SaveChanges();

            return Ok();
        }
    }
}
