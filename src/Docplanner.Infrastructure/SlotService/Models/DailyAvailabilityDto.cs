namespace Docplanner.Infrastructure.SlotService.Models
{
    public class DailyAvailabilityDto
    {
        public List<BusySlotDto>? BusySlots { get; set; }
        public WorkPeriodDto? WorkPeriod { get; set; }
    }
}