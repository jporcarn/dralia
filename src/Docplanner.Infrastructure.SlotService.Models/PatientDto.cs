namespace Docplanner.Infrastructure.SlotService.Models
{
    public class PatientDto
    {
        public string? Email { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? SecondName { get; set; }
    }
}