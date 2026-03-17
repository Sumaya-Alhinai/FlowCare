using FlowCare.Models;

namespace FlowCare.Interfaces
{
    public interface ISlotService
    {
        Task<List<Slot>> GetAllAsync();
        Task<Slot> CreateAsync(string branchId, string serviceTypeId,
                               DateTime start, DateTime end);
        Task<bool> UpdateAsync(string id, DateTime start, DateTime end);
        Task<bool> SoftDeleteAsync(string id);
    }
}