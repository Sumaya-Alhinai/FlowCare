using FlowCare.Data;
using FlowCare.DTOs;
using FlowCare.Interfaces;
using FlowCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlowCare.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;

        public AdminController(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpPut("retention")]
        public async Task<IActionResult> SetRetention([FromBody] RetentionDto dto)
        {
            if (dto.Days <= 0)
                return BadRequest(new { message = "Days must be greater than 0." });

            var config = await _context.Configs
                .FirstOrDefaultAsync(c => c.Key == "RetentionDays");

            if (config == null)
            {
                _context.Configs.Add(new Config
                {
                    Key = "RetentionDays",
                    Value = dto.Days.ToString()
                });
            }
            else
            {
                config.Value = dto.Days.ToString();
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Retention period set to {dto.Days} days." });
        }

        [HttpPost("cleanup")]
        public async Task<IActionResult> Cleanup()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var config = await _context.Configs
                .FirstOrDefaultAsync(c => c.Key == "RetentionDays");

            var days = config != null ? int.Parse(config.Value) : 30;
            var cutoff = DateTime.UtcNow.AddDays(-days);

            var expiredSlots = await _context.Slots
                .IgnoreQueryFilters()
                .Where(s => s.IsDeleted && s.DeletedAt < cutoff)
                .ToListAsync();

            if (!expiredSlots.Any())
                return Ok(new { message = "No expired slots to clean up.", deleted = 0 });

            foreach (var slot in expiredSlots)
            {
                var appointments = await _context.Appointments
                    .Where(a => a.SlotId == slot.Id)
                    .ToListAsync();

                foreach (var appt in appointments)
                    appt.SlotId = null;
            }

            await _context.SaveChangesAsync();

            _context.Slots.RemoveRange(expiredSlots);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                "HARD_DELETE_SLOTS", userId, userRole,
                "Slot", "bulk",
                new { DeletedCount = expiredSlots.Count, CutoffDate = cutoff });

            return Ok(new
            {
                message = "Cleanup completed.",
                deleted = expiredSlots.Count
            });
        }
    }
}