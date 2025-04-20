namespace Docplanner.Api.Models
{

    public class WeeklySlotsResponse
    {
        public List<DailySlotsResponse> Days { get; set; } = new();
        public FacilityResponse? Facility { get; set; }
        public int SlotDurationMinutes { get; set; }
    }
}