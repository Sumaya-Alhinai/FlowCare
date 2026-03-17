namespace FlowCare.Models
{
    public class Slot
    {
        public string Id { get; set; } = default!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public string BranchId { get; set; } = default!;
        public Branch? Branch { get; set; }
        public string ServiceTypeId { get; set; } = default!;
        public ServiceType? ServiceType { get; set; }
        public string? StaffId { get; set; }
        public Staff? Staff { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Appointment? Appointment { get; set; }
    }
}