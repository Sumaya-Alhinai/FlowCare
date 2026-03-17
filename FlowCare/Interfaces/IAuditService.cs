namespace FlowCare.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(
            string actionType,
            string userId,
            string role,
            string entity,
            string targetId,
            object? metadata = null,
            string? branchId = null); 
    }
}