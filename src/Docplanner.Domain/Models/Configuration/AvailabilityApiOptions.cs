namespace Docplanner.Domain.Models.Configuration
{
    public class AvailabilityApiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;

        public CredentialsOptions Credentials { get; set; } = new();

        public class CredentialsOptions
        {
            public string Password { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
        }
    }
}