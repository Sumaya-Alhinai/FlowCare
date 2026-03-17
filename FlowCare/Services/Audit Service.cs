using FlowCare.Data;
using FlowCare.Interfaces;
using FlowCare.Models;
using System.Text.Json;

namespace FlowCare.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;

        public AuditService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            string actionType,
            string userId,
            string role,
            string entity,
            string targetId,
            object? metadata = null,
            string? branchId = null) 
        {
            var log = new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                ActionType = actionType,
                UserId = userId,
                UserRole = role,
                TargetEntity = entity,
                TargetId = targetId,
                BranchId = branchId, 
                Metadata = metadata != null
                    ? JsonSerializer.Serialize(metadata)
                    : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}