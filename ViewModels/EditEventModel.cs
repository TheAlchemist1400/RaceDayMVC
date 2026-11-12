
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    // Holds the information required to edit an existing event.
    public class EditEventViewModel
    {
        public int EventId { get; set; }

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

        // Available locations for the dropdown.
        public List<EventLocationViewModel> Locations { get; set; } = new();
    }
}
