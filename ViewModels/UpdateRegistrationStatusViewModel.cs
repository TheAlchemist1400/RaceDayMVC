
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class UpdateRegistrationStatusViewModel
    {
        [Range(1, int.MaxValue)]
        public int RegistrationId { get; set; }

        [Range(1, int.MaxValue)]
        public int EventId { get; set; }

        [Required]
        [RegularExpression(
            "^(Pending|Confirmed|Cancelled)$",
            ErrorMessage = "Invalid registration status.")]
        public string RegistrationStatus { get; set; } = string.Empty;
    }
}
