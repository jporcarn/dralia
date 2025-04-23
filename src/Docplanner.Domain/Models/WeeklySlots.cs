namespace Docplanner.Domain.Models
{
    public class WeeklySlots
    {
        public List<DailySlots> Days { get; set; } = new();
        public Facility Facility { get; set; } = new();
        public int SlotDurationMinutes { get; set; }
    }
}