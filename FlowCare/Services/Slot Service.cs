using FlowCare.Data;
using FlowCare.Interfaces;
using FlowCare.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Services
{
    public class SlotService : ISlotService
    {
        private readonly AppDbContext _context;

        public SlotService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Slot>> GetAllAsync()
        {
            return await _context.Slots
                .AsNoTracking()
                .Include(s => s.Branch)
                .Include(s => s.ServiceType)
                .ToListAsync();
        }

        public async Task<Slot> CreateAsync(string branchId, string serviceTypeId,
                                            DateTime start, DateTime end)
        {
            ValidateTimeRange(start, end);

            var overlap = await _context.Slots
                .AnyAsync(s => s.BranchId == branchId &&
                               !s.IsDeleted &&
                               s.StartTime < end &&
                               s.EndTime > start);

            if (overlap) throw new Exception("Slot overlaps");

            var slot = new Slot
            {
                Id = Guid.NewGuid().ToString(),
                BranchId = branchId,
                ServiceTypeId = serviceTypeId,
                StartTime = start,
                EndTime = end,
                IsDeleted = false
            };

            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();
            return slot;
        }

        public async Task<bool> UpdateAsync(string id, DateTime start, DateTime end)
        {
            var slot = await _context.Slots.FirstOrDefaultAsync(s => s.Id == id);
            if (slot == null || slot.IsDeleted) return false;

            var overlap = await _context.Slots
                .AnyAsync(s => s.Id != id &&
                               s.BranchId == slot.BranchId &&
                               !s.IsDeleted &&
                               s.StartTime < end &&
                               s.EndTime > start);

            if (overlap) return false;

            slot.StartTime = start;
            slot.EndTime = end;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(string id)
        {
            var slot = await _context.Slots.FirstOrDefaultAsync(s => s.Id == id);
            if (slot == null || slot.IsDeleted) return false;

            slot.IsDeleted = true;
            slot.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static void ValidateTimeRange(DateTime start, DateTime end)
        {
            if (start < DateTime.UtcNow) throw new ArgumentException("Invalid start");
            if (end <= start) throw new ArgumentException("Invalid end");
        }
    }
}