using System.ComponentModel.DataAnnotations;

namespace Docplanner.Api.Models
{
    public class PatientRequest
    {
        public string? Email { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string? SecondName { get; set; }
    }
}