namespace Docplanner.Domain.Models
{
    public class BookSlot
    {
        public string Comments { get; set; } = string.Empty;

        public Patient Patient { get; set; } = new();

        public DateTime Start { get; set; }
    }
}