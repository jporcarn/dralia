namespace Docplanner.Domain.Models
{
    public class Slot
    {
        public bool Busy { get; set; }
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
    }
}