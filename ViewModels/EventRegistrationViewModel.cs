namespace RaceDay.Mvc.ViewModels
{
    public class EventRegistrationViewModel
    {
        public int RegistrationId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public int EventId { get; set; }
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public int BibNumber { get; set; }
        public DateOnly RegistrationDate { get; set; }
        public string RegistrationStatus { get; set; } = string.Empty;
    }
}
