
namespace RaceDay.Mvc.ViewModels
{
    // Represents one event returned by the RaceDay API.
    public class EventViewModel
    {
        public int EventId { get; set; }

        public string EventsName { get; set; } = string.Empty;

        // DateOnly matches the API's DateOnly property.
        public DateOnly EventDate { get; set; }

        public int MaxParticipants { get; set; }

        public string? BannerImageUrl { get; set; }

        public int LocationId { get; set; }

        public string Location { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Province { get; set; } = string.Empty;

        public string Organizer { get; set; } = string.Empty;
    }
}
