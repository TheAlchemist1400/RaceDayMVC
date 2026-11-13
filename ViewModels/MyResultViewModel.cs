
namespace RaceDay.Mvc.ViewModels
{
    // Represents a Participant's race result
    // returned by the RaceDay API.
    public class MyResultViewModel
    {
        public int ResultId { get; set; }

        public int RegistrationId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public DateOnly EventDate { get; set; }

        public string ActivityName { get; set; } = string.Empty;

        public TimeOnly? FinishTime { get; set; }

        public int? Position { get; set; }

        public string ResultStatus { get; set; } = string.Empty;
    }
}
