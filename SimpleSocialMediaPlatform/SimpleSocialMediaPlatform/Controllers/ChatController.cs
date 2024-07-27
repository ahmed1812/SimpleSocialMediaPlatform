using Microsoft.AspNetCore.Mvc;
using SimpleSocialMediaPlatform.Data;
using SimpleSocialMediaPlatform.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace SimpleSocialMediaPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage model)
        {
            if (ModelState.IsValid)
            {
                model.Timestamp = DateTime.Now;
                _context.ChatMessages.Add(model);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest(ModelState);
        }

        [HttpGet("{fromUserId}/{toUserId}")]
        public IActionResult GetMessages(string fromUserId, string toUserId)
        {
            var messages = _context.ChatMessages
                .Where(m => (m.FromUserId == fromUserId && m.ToUserId == toUserId) ||
                            (m.FromUserId == toUserId && m.ToUserId == fromUserId))
                .OrderBy(m => m.Timestamp)
                .ToList();

            return Ok(messages);
        }

        [HttpGet("new/{userId}")]
        public IActionResult GetNewMessages(string userId)
        {
            var messages = _context.ChatMessages
                .Where(m => m.ToUserId == userId && !m.IsRead)
                .OrderBy(m => m.Timestamp)
                .ToList();

            return Ok(messages);
        }

        [HttpPost("markAsRead")]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] int[] messageIds)
        {
            var messages = _context.ChatMessages.Where(m => messageIds.Contains(m.Id)).ToList();
            messages.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
