using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Role { get; set; } = "Participant";

        // Participant details
        [StringLength(50)]
        public string? Name { get; set; }

        [StringLength(50)]
        public string? Lastname { get; set; }

        // Organiser details
        [StringLength(100)]
        public string? OrganizationName { get; set; }

        // Shared details
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
