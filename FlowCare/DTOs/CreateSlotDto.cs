namespace FlowCare.DTOs
{
    public class CreateSlotDto
    {
        public string BranchId { get; set; } = default!;
        public string ServiceTypeId { get; set; } = default!;
        public string? StaffId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}