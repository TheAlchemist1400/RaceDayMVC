
namespace RaceDay.Mvc.ViewModels
{
    // Represents a location returned by the RaceDay API.
    public class EventLocationViewModel
    {
        public int LocationId { get; set; }

        public string City { get; set; } = string.Empty;

        public string Province { get; set; } = string.Empty;

        public string Venue { get; set; } = string.Empty;
    }
}
