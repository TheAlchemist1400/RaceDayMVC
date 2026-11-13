
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    // Represents the information needed to enrol in an activity.
    public class CreateEnrolmentViewModel
    {
        [Range(1, int.MaxValue)]
        public int EventId { get; set; }

        [Range(1, int.MaxValue)]
        public int ActivityId { get; set; }
    }
}
