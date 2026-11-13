
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class ParticipantProfileViewModel
    {
        public int ParticipantId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string Lastname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public string? ProfileImageUrl { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
