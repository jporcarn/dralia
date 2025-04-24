namespace Docplanner.Infrastructure.SlotService.Models
{
    public class WeeklyAvailabilityDto
    {
        public FacilityDto? Facility { get; set; }
        public int SlotDurationMinutes { get; set; }

        public DailyAvailabilityDto? Monday { get; set; }
        public DailyAvailabilityDto? Tuesday { get; set; }
        public DailyAvailabilityDto? Wednesday { get; set; }
        public DailyAvailabilityDto? Thursday { get; set; }
        public DailyAvailabilityDto? Friday { get; set; }
    }
}