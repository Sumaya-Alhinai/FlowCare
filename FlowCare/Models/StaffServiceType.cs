namespace FlowCare.Models
{
    public class StaffServiceType
    {
        public string StaffId { get; set; } = default!;
        public Staff? Staff { get; set; }
        public string ServiceTypeId { get; set; } = default!;
        public ServiceType? ServiceType { get; set; }
    }
}