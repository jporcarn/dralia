namespace Docplanner.Api.Handlers
{
    public class BasicAuthHandler : DelegatingHandler
    {
        private readonly string _username;
        private readonly string _password;

        public BasicAuthHandler(string username, string password)
        {
            _username = username;
            _password = password;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_username}:{_password}"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
