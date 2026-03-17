using System.ComponentModel.DataAnnotations;
using FlowCare.Enums;
namespace FlowCare.Models
{
    public class User
    {
        public string Id { get; set; } = default!;

        [Required]
        public string Username { get; set; } = default!;

        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;

        [Required]
        public UserRole Role { get; set; }

        public string? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public bool IsActive { get; set; } = true;
        public string? FullName { get; set; }
    }
}