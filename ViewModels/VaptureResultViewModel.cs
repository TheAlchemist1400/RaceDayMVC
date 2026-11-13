
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class CaptureResultViewModel
    {
        [Range(1, int.MaxValue)]
        public int RegistrationId { get; set; }

        // Used to return to the correct event.
        [Range(1, int.MaxValue)]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Finish time is required.")]
        public TimeOnly? FinishTime { get; set; }

        [Required(ErrorMessage = "Finishing position is required.")]
        [Range(1, int.MaxValue)]
        public int? Position { get; set; }

        [Required]
        [StringLength(20)]
        public string ResultStatus { get; set; } = "Finished";
    }
}
