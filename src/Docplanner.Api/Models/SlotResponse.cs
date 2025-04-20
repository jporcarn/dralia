namespace Docplanner.Api.Models
{
    public class SlotResponse
    {
        public bool Busy { get; set; }
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
    }
}