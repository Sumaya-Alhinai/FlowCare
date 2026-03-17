using FlowCare.Helpers;
using FlowCare.Models;

namespace FlowCare.Interfaces
{
    public interface IAppointmentService
    {
        Task<Appointment?> BookAsync(string userId, string slotId);
        Task<bool> CancelAsync(string userId, string appointmentId);
        Task<bool> RescheduleAsync(string userId, string appointmentId, string newSlotId);
        Task<bool> UpdateStatusAsync(string appointmentId, string status);
        Task<Appointment?> GetByIdAsync(string appointmentId); 
        Task SetAttachmentAsync(string appointmentId, string path); 
        Task<PagedResult<Appointment>> GetAllAsync(
            int page,
            int size,
            string? term,
            string role,
            string? branchId,
            string? userId);
    }
}