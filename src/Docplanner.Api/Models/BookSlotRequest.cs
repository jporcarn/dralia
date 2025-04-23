using System.ComponentModel.DataAnnotations;

namespace Docplanner.Api.Models
{
    public class BookSlotRequest
    {
        public string Comments { get; set; } = string.Empty;

        [Required]
        public PatientRequest Patient { get; set; } = new();

        [Required]
        public DateTime Start { get; set; }
    }
}