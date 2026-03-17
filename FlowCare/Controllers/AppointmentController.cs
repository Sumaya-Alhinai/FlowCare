using System.Security.Claims;
using FlowCare.DTOs;
using FlowCare.Interfaces;
using FlowCare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAuditService _auditService;
        private readonly FileService _fileService;

        public AppointmentController(
            IAppointmentService appointmentService,
            IAuditService auditService,
            FileService fileService)
        {
            _appointmentService = appointmentService;
            _auditService = auditService;
            _fileService = fileService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,BranchManager,Staff,Customer")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? term = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
            var branchId = User.FindFirst("BranchId")?.Value;

            var result = await _appointmentService.GetAllAsync(
                page, size, term, userRole, branchId, userId);

            var response = new
            {
                result.Total,
                Results = result.Results.Select(a => new
                {
                    a.Id,
                    a.Status,
                    a.CreatedAt,
                    a.BranchId,
                    a.ServiceTypeId,
                    a.SlotId,
                    a.AttachmentPath,
                    Customer = a.Customer == null ? null : new
                    {
                        a.Customer.Id,
                        a.Customer.Name,
                        a.Customer.Email,
                        a.Customer.Phone
                    },
                    Staff = a.Staff == null ? null : new
                    {
                        a.Staff.Id,
                        a.Staff.FullName,
                        a.Staff.Email
                    },
                    Slot = a.Slot == null ? null : new
                    {
                        a.Slot.Id,
                        a.Slot.StartTime,
                        a.Slot.EndTime
                    }
                })
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,BranchManager,Staff,Customer")]
        public async Task<IActionResult> GetById(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var appointment = await _appointmentService.GetByIdAsync(id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found." });

            if (userRole == "Customer" && appointment.CustomerId != userId)
                return Forbid();

            return Ok(new
            {
                appointment.Id,
                appointment.Status,
                appointment.CreatedAt,
                appointment.BranchId,
                appointment.ServiceTypeId,
                appointment.SlotId,
                appointment.AttachmentPath,
                Customer = appointment.Customer == null ? null : new
                {
                    appointment.Customer.Id,
                    appointment.Customer.Name,
                    appointment.Customer.Email
                },
                Staff = appointment.Staff == null ? null : new
                {
                    appointment.Staff.Id,
                    appointment.Staff.FullName
                },
                Slot = appointment.Slot == null ? null : new
                {
                    appointment.Slot.Id,
                    appointment.Slot.StartTime,
                    appointment.Slot.EndTime
                }
            });
        }

        [HttpPost("book")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Book([FromForm] BookAppointmentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var appointment = await _appointmentService.BookAsync(userId, dto.SlotId);
            if (appointment == null)
                return BadRequest(new { message = "Booking failed. Slot may be unavailable or you have reached the daily limit of 3 bookings." });

            if (dto.Attachment != null && dto.Attachment.Length > 0)
            {
                try
                {
                    var path = await _fileService.SaveAppointmentAttachment(dto.Attachment);
                    await _appointmentService.SetAttachmentAsync(appointment.Id, path);
                    appointment.AttachmentPath = path;
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            await _auditService.LogAsync(
                "APPOINTMENT_CREATED", userId, userRole,
                "Appointment", appointment.Id,
                new { dto.SlotId });

            return Ok(new
            {
                appointment.Id,
                appointment.SlotId,
                appointment.BranchId,
                appointment.ServiceTypeId,
                appointment.Status,
                appointment.AttachmentPath,
                appointment.CreatedAt
            });
        }

        [HttpDelete("{id}/cancel")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Cancel(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var result = await _appointmentService.CancelAsync(userId, id);
            if (!result) return NotFound(new { message = "Appointment not found." });

            await _auditService.LogAsync(
                "APPOINTMENT_CANCELLED", userId, userRole,
                "Appointment", id, null);

            return Ok(new { message = "Appointment cancelled successfully." });
        }

        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Reschedule(string id, [FromBody] RescheduleDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var result = await _appointmentService.RescheduleAsync(userId, id, dto.NewSlotId);
            if (!result)
                return BadRequest(new { message = "Reschedule failed. You may have reached the daily limit of 2 reschedules." });

            await _auditService.LogAsync(
                "APPOINTMENT_RESCHEDULED", userId, userRole,
                "Appointment", id,
                new { dto.NewSlotId });

            return Ok(new { message = "Appointment rescheduled successfully." });
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,BranchManager,Staff")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var result = await _appointmentService.UpdateStatusAsync(id, dto.Status);
            if (!result) return NotFound(new { message = "Appointment not found." });

            await _auditService.LogAsync(
                "APPOINTMENT_STATUS_UPDATED", userId, userRole,
                "Appointment", id,
                new { dto.Status });

            return Ok(new { message = "Status updated successfully." });
        }

        [HttpGet("{id}/attachment")]
        [Authorize(Roles = "Admin,BranchManager,Staff,Customer")]
        public async Task<IActionResult> GetAttachment(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var appointment = await _appointmentService.GetByIdAsync(id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found." });

            if (userRole == "Customer" && appointment.CustomerId != userId)
                return Forbid();

            if (string.IsNullOrEmpty(appointment.AttachmentPath))
                return NotFound(new { message = "No attachment found." });

            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "appointment_attachments");

            var filePath = Path.Combine(folder, appointment.AttachmentPath);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File not found." });

            var ext = Path.GetExtension(appointment.AttachmentPath).ToLower();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, contentType);
        }
    }
}