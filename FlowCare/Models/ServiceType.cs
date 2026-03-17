namespace FlowCare.Models
{
    public class ServiceType
    {
        public ServiceType()
        {
            Slots = new List<Slot>();
        }

        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public string BranchId { get; set; } = default!;
        public Branch? Branch { get; set; }

        public ICollection<Slot> Slots { get; set; }
    }
}