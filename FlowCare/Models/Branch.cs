namespace FlowCare.Models
{
    public class Branch
    {
        public Branch()
        {
            Staff = new List<Staff>();
            ServiceTypes = new List<ServiceType>();
            Slots = new List<Slot>();
        }

        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Timezone { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Staff> Staff { get; set; }
        public ICollection<ServiceType> ServiceTypes { get; set; }
        public ICollection<Slot> Slots { get; set; }
    }
}