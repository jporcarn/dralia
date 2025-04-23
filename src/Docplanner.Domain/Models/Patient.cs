namespace Docplanner.Domain.Models
{
    public class Patient
    {
        public string? Email { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? SecondName { get; set; }
    }
}