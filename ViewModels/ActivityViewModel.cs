
namespace RaceDay.Mvc.ViewModels
{
    // Represents an activity/category within an event.
    public class ActivityViewModel
    {
        public int ActivityId { get; set; }

        public int EventId { get; set; }

        public string ActivityName { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }

        public decimal EntryFee { get; set; }

        public TimeOnly StartTime { get; set; }

        public int MaximumEntrants { get; set; }
    }
}
