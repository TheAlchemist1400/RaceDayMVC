
namespace RaceDay.Mvc.ViewModels
{
    // Represents an enrolment returned by the API.
    public class MyEnrolmentViewModel
    {
        public int RegistrationId { get; set; }

        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public int ActivityId { get; set; }

        public string ActivityName { get; set; } = string.Empty;

        public DateOnly RegistrationDate { get; set; }

        public int BibNumber { get; set; }

        public string RegistrationStatus { get; set; } = string.Empty;
    }
}
