namespace FlowCare.Models
{
    public class AuditLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = default!;
        public User? User { get; set; }
        public string ActionType { get; set; } = default!;
        public string UserRole { get; set; } = default!;
        public string TargetEntity { get; set; } = default!;
        public string TargetId { get; set; } = default!;
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? BranchId { get; set; }
    }
}