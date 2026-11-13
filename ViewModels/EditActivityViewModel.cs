
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class EditActivityViewModel
    {
        public int ActivityId { get; set; }

        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter an activity name.")]
        [StringLength(100)]
        public string ActivityName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999.99",
            ErrorMessage = "Distance must be greater than zero.")]
        public decimal DistanceKm { get; set; }

        [Range(typeof(decimal), "0", "999999.99",
            ErrorMessage = "Entry fee cannot be negative.")]
        public decimal EntryFee { get; set; }

        public TimeOnly StartTime { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Maximum entrants must be at least 1.")]
        public int MaximumEntrants { get; set; }
    }
}
