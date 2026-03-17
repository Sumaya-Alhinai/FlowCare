using FlowCare.Data;
using FlowCare.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Controllers
{
    [ApiController]
    [Route("api/branches")]
    public class BranchesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BranchesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var branches = await _context.Branches
                .Where(b => b.IsActive)
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.City,
                    b.Address,
                    b.Timezone
                })
                .ToListAsync();

            return Ok(branches);
        }

        [HttpGet("{id}/services")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServices(string id)
        {
            var branchExists = await _context.Branches
                .AnyAsync(b => b.Id == id && b.IsActive);

            if (!branchExists)
                return NotFound(new { message = "Branch not found." });

            var services = await _context.ServiceTypes
                .Where(s => s.BranchId == id && s.IsActive)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.DurationMinutes
                })
                .ToListAsync();

            return Ok(services);
        }

        [HttpGet("{id}/slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSlots(
            string id,
            [FromQuery] string? serviceTypeId = null,
            [FromQuery] DateTimeOffset? date = null)
        {
            var branchExists = await _context.Branches
                .AnyAsync(b => b.Id == id && b.IsActive);

            if (!branchExists)
                return NotFound(new { message = "Branch not found." });

            var query = _context.Slots
                .Include(s => s.ServiceType)
                .Where(s => s.BranchId == id && s.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(serviceTypeId))
                query = query.Where(s => s.ServiceTypeId == serviceTypeId);

            if (date.HasValue)
            {
                var start = DateTime.SpecifyKind(
                    date.Value.UtcDateTime.Date, DateTimeKind.Utc);
                var end = start.AddDays(1);
                query = query.Where(s =>
                    s.StartTime >= start && s.StartTime < end);
            }

            var slots = await query
                .Select(s => new
                {
                    s.Id,
                    s.StartTime,
                    s.EndTime,
                    s.Capacity,
                    s.IsActive,
                    ServiceType = new
                    {
                        s.ServiceType.Id,
                        s.ServiceType.Name,
                        s.ServiceType.DurationMinutes
                    },
                    IsBooked = s.Appointment != null
                })
                .ToListAsync();

            return Ok(slots);
        }

        [HttpGet("{id}/queue")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQueuePosition(
            string id,
            [FromQuery] string appointmentId)
        {
            var branchExists = await _context.Branches
                .AnyAsync(b => b.Id == id && b.IsActive);

            if (!branchExists)
                return NotFound(new { message = "Branch not found." });

            var today = DateTime.SpecifyKind(
                DateTime.UtcNow.Date, DateTimeKind.Utc);

            var todayAppointments = await _context.Appointments
                .Include(a => a.Slot)
                .Where(a =>
                    a.BranchId == id &&
                    a.Status == AppointmentStatus.Booked &&
                    a.Slot != null &&
                    a.Slot.StartTime >= today &&
                    a.Slot.StartTime < today.AddDays(1))
                .OrderBy(a => a.Slot!.StartTime)
                .ToListAsync();

            var position = todayAppointments
                .FindIndex(a => a.Id == appointmentId) + 1;

            if (position == 0)
                return NotFound(new { message = "Appointment not found in queue." });

            var appointment = todayAppointments[position - 1];
            var avgDuration = appointment.Slot?.EndTime
                .Subtract(appointment.Slot.StartTime).TotalMinutes ?? 15;

            return Ok(new
            {
                AppointmentId = appointmentId,
                BranchId = id,
                QueuePosition = position,
                TotalInQueue = todayAppointments.Count,
                EstimatedWaitMinutes = (position - 1) * avgDuration
            });
        }
    }
}