namespace Docplanner.Infrastructure.SlotService.Models
{
    public class TakeSlotDto
    {
        public string Comments { get; set; } = string.Empty;
        public DateTime End { get; set; }
        public Guid FacilityId { get; set; }
        public PatientDto? Patient { get; set; }
        public DateTime Start { get; set; }
    }
}