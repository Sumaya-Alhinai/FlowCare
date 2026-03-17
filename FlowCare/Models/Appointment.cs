using FlowCare.Enums;
namespace FlowCare.Models
{
    public class Appointment
    {
        public string Id { get; set; } = default!;
        public string CustomerId { get; set; } = default!;
        public Customer? Customer { get; set; }
        public string SlotId { get; set; } = default!;
        public Slot? Slot { get; set; }
        public string? StaffId { get; set; }
        public Staff? Staff { get; set; }
        public string BranchId { get; set; } = default!;
        public Branch? Branch { get; set; }
        public string ServiceTypeId { get; set; } = default!;
        public ServiceType? ServiceType { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
        public string? AttachmentPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}