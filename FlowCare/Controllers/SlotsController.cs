using System.Security.Claims;
using FlowCare.Data;
using FlowCare.DTOs;
using FlowCare.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Controllers
{
    [ApiController]
    [Route("api/slots")]
    public class SlotsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISlotService _slotService;
        private readonly IAuditService _auditService;

        public SlotsController(
            AppDbContext context,
            ISlotService slotService,
            IAuditService auditService)
        {
            _context = context;
            _slotService = slotService;
            _auditService = auditService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var slots = await _context.Slots
                .Include(s => s.Branch)
                .Include(s => s.ServiceType)
                .Select(s => new
                {
                    s.Id,
                    s.StartTime,
                    s.EndTime,
                    s.Capacity,
                    s.IsActive,
                    Branch = new
                    {
                        s.Branch.Id,
                        s.Branch.Name,
                        s.Branch.City
                    },
                    ServiceType = new
                    {
                        s.ServiceType.Id,
                        s.ServiceType.Name,
                        s.ServiceType.DurationMinutes
                    }
                })
                .ToListAsync();

            return Ok(slots);
        }

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDeleted()
        {
            var slots = await _context.Slots
                .IgnoreQueryFilters()
                .Where(s => s.IsDeleted)
                .Include(s => s.Branch)
                .Include(s => s.ServiceType)
                .Select(s => new
                {
                    s.Id,
                    s.StartTime,
                    s.EndTime,
                    s.IsDeleted,
                    s.DeletedAt,
                    s.Capacity,
                    Branch = new
                    {
                        s.Branch.Id,
                        s.Branch.Name,
                        s.Branch.City
                    },
                    ServiceType = new
                    {
                        s.ServiceType.Id,
                        s.ServiceType.Name,
                        s.ServiceType.DurationMinutes
                    }
                })
                .ToListAsync();

            return Ok(slots);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,BranchManager")]
        public async Task<IActionResult> Create([FromBody] CreateSlotDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var slot = await _slotService.CreateAsync(
                dto.BranchId,
                dto.ServiceTypeId,
                dto.StartTime,
                dto.EndTime);

            await _auditService.LogAsync(
                "SLOT_CREATED", userId, userRole,
                "Slot", slot.Id,
                new { slot.BranchId, slot.ServiceTypeId },
                slot.BranchId);

            return Ok(new
            {
                slot.Id,
                slot.StartTime,
                slot.EndTime,
                slot.BranchId,
                slot.ServiceTypeId
            });
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,BranchManager")]
        public async Task<IActionResult> CreateBulk([FromBody] List<CreateSlotDto> dtos)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            if (dtos == null || dtos.Count == 0)
                return BadRequest(new { message = "No slots provided." });

            var createdSlots = new List<object>();

            foreach (var dto in dtos)
            {
                try
                {
                    var slot = await _slotService.CreateAsync(
                        dto.BranchId,
                        dto.ServiceTypeId,
                        dto.StartTime,
                        dto.EndTime);

                    await _auditService.LogAsync(
                        "SLOT_CREATED", userId, userRole,
                        "Slot", slot.Id,
                        new { slot.BranchId, slot.ServiceTypeId },
                        slot.BranchId);

                    createdSlots.Add(new
                    {
                        slot.Id,
                        slot.StartTime,
                        slot.EndTime,
                        slot.BranchId,
                        slot.ServiceTypeId
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            return Ok(new
            {
                Created = createdSlots.Count,
                Slots = createdSlots
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,BranchManager")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateSlotDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var slot = await _context.Slots
                .FirstOrDefaultAsync(s => s.Id == id);

            var result = await _slotService.UpdateAsync(id, dto.StartTime, dto.EndTime);
            if (!result) return NotFound(new { message = "Slot not found." });

            await _auditService.LogAsync(
                "SLOT_UPDATED", userId, userRole,
                "Slot", id, null,
                slot?.BranchId);

            return Ok(new { message = "Slot updated successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,BranchManager")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var slot = await _context.Slots
                .FirstOrDefaultAsync(s => s.Id == id);

            var result = await _slotService.SoftDeleteAsync(id);
            if (!result) return NotFound(new { message = "Slot not found." });

            await _auditService.LogAsync(
                "SLOT_SOFT_DELETED", userId, userRole,
                "Slot", id, null,
                slot?.BranchId);

            return Ok(new { message = "Slot soft deleted successfully." });
        }
    }
}