using FlowCare.Data;
using FlowCare.Enums;
using FlowCare.Helpers;
using FlowCare.Interfaces;
using FlowCare.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppDbContext _context;

        public AppointmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment?> BookAsync(string userId, string slotId)
        {
            var slot = await _context.Slots
                .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsDeleted);

            if (slot == null) return null;

            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var todayBookings = await _context.Appointments
                .CountAsync(a =>
                    a.CustomerId == userId &&
                    a.CreatedAt >= today &&
                    a.CreatedAt < today.AddDays(1) &&
                    a.Status != AppointmentStatus.Cancelled);

            if (todayBookings >= 3) return null;

            var exists = await _context.Appointments
                .AnyAsync(a => a.SlotId == slotId);

            if (exists) return null;

            var appointment = new Appointment
            {
                Id = Guid.NewGuid().ToString(),
                CustomerId = userId,
                SlotId = slotId,
                BranchId = slot.BranchId,
                ServiceTypeId = slot.ServiceTypeId,
                StaffId = slot.StaffId,
                Status = AppointmentStatus.Booked,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<bool> CancelAsync(string userId, string appointmentId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId && a.CustomerId == userId);

            if (appointment == null) return false;

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RescheduleAsync(
            string userId, string appointmentId, string newSlotId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId && a.CustomerId == userId);

            if (appointment == null) return false;

            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var todayReschedules = await _context.AuditLogs
                .CountAsync(a =>
                    a.UserId == userId &&
                    a.ActionType == "APPOINTMENT_RESCHEDULED" &&
                    a.CreatedAt >= today &&
                    a.CreatedAt < today.AddDays(1));

            if (todayReschedules >= 2) return false;

            appointment.SlotId = newSlotId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(string appointmentId, string status)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return false;

            appointment.Status = Enum.Parse<AppointmentStatus>(status, ignoreCase: true);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Appointment?> GetByIdAsync(string appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Staff)
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        public async Task SetAttachmentAsync(string appointmentId, string path)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return;

            appointment.AttachmentPath = path;
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<Appointment>> GetAllAsync(
            int page,
            int size,
            string? term,
            string role,
            string? branchId,
            string? userId)
        {
            var query = _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Slot)
                .Include(a => a.Staff)
                .AsQueryable();

            if (role == "Customer")
                query = query.Where(a => a.CustomerId == userId);
            else if (role == "Staff")
                query = query.Where(a => a.StaffId == userId);
            else if (role == "BranchManager" && branchId != null)
                query = query.Where(a => a.BranchId == branchId);

            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(a =>
                    a.Id.Contains(term) ||
                    a.Status.ToString().Contains(term));

            var total = await query.CountAsync();

            var results = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return new PagedResult<Appointment>
            {
                Results = results,
                Total = total
            };
        }
    }
}