
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    // Represents the form used to create an activity.
    public class CreateActivityViewModel
    {
        // Identifies the event this activity belongs to.
        [Range(1, int.MaxValue)]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter an activity name.")]
        [StringLength(100)]
        public string ActivityName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999.99",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true,
            ErrorMessage = "Distance must be greater than zero.")]
        public decimal DistanceKm { get; set; }

        [Range(typeof(decimal), "0", "999999.99",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true,
            ErrorMessage = "Entry fee cannot be negative.")]
        public decimal EntryFee { get; set; }

        public TimeOnly StartTime { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Maximum entrants must be at least 1.")]
        public int MaximumEntrants { get; set; }
    }
}
