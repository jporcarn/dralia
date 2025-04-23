namespace Docplanner.Domain.Models
{
    public class DailySlots
    {
        public DateOnly Date { get; set; }
        public string? DayOfWeek { get; set; }
        public List<Slot> Slots { get; set; } = new();
        public WorkPeriod? WorkPeriod { get; set; }
    }
}