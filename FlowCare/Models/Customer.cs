using System.ComponentModel.DataAnnotations;
namespace FlowCare.Models
{
    public class Customer
    {
        public Customer()
        {
            Appointments = new List<Appointment>();
        }

        public string Id { get; set; } = default!;

        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;

        [Required]
        public string IdImagePath { get; set; } = default!;

        public string? Phone { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
    }
}