namespace Docplanner.Api.Models
{
    public class DailySlotsResponse
    {
        public DateOnly Date { get; set; }
        public string? DayOfWeek { get; set; }
        public List<SlotResponse> Slots { get; set; } = new();
        public WorkPeriodResponse? WorkPeriod { get; set; }
    }
}