
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    // Holds the information required to create an event.
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Please enter an event name.")]
        [StringLength(50)]
        public string EventsName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an event date.")]
        public DateOnly EventDate { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Maximum participants must be at least 1.")]
        public int MaxParticipants { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a valid event location.")]
        public int LocationId { get; set; }

        // Stores locations retrieved from the API.
        // This list populates the location dropdown.
        public List<EventLocationViewModel> Locations { get; set; } = new();
    }
}
