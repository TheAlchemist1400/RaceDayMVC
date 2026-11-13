namespace RaceDay.Mvc.ViewModels
{
    // Combines event details with its available activities.
    public class EventDetailsViewModel
    {
        public EventViewModel Event { get; set; } = new();

        public List<ActivityViewModel> Activities { get; set; } = new();

        // Indicates whether the logged-in Participant
        // already has a registration for this event.
        public bool IsEnrolled { get; set; }

        // The Participant's current registration status.
        public string? EnrolmentStatus { get; set; }
    }
}
