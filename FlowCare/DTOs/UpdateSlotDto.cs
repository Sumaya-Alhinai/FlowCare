namespace FlowCare.DTOs
{
    public class UpdateSlotDto
    {
        public string Id { get; set; } = default!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}