namespace FlowCare.DTOs
{
    public class BookAppointmentDto
    {
        public string SlotId { get; set; } = null!;
        public IFormFile? Attachment { get; set; }
    }
}