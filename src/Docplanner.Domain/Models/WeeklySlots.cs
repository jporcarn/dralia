namespace Docplanner.Api.Models
{

    public class WeeklySlots
    {
        public List<DailySlots> Days { get; set; } = new();
        public Facility? Facility { get; set; }
        public int SlotDurationMinutes { get; set; }
    }
}