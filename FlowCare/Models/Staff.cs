namespace FlowCare.Models
{
    public class Staff
    {
        public Staff()
        {
            Appointments = new List<Appointment>();
            StaffServiceTypes = new List<StaffServiceType>();
        }

        public string Id { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string BranchId { get; set; } = default!;
        public Branch Branch { get; set; } = default!;

        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<StaffServiceType> StaffServiceTypes { get; set; }
    }
}