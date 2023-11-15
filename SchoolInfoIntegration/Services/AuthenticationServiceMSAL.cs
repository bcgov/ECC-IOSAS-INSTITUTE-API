
using ECC.Institute.CRM.IntegrationAPI.Models;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace ECC.Institute.CRM.IntegrationAPI
{
    /// <summary>
    /// New and preferred Authentication Service with MSAL library
    /// </summary>
    public class AuthenticationServiceMSAL : ID365AuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly List<D365AppSettings> _appSettings;
        public List<D365AppSettings> D365AppSettings { get { return _appSettings; } }

      // D365AppSettings ID365AuthenticationService.D365AppSettings => throw new NotImplementedException();

        public AuthenticationServiceMSAL(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var authSettingsSection = _configuration.GetSection("D365AppSettings");
            _appSettings = authSettingsSection.Get<List<D365AppSettings>>();
        } 

        public async Task<HttpClient> GetHttpClient(string applicationName)
        {
            var appSettings = _appSettings.FirstOrDefault(a => a.Name.Equals(applicationName, StringComparison.InvariantCultureIgnoreCase));
            if (appSettings == null)
            {
                throw new Exception($"Application {applicationName} is not supported.");
            }
            var accessToken = await GetAccessToken(appSettings.BaseUrl,
                                                    appSettings.ClientId,
                                                    appSettings.ClientSecret,
                                                    appSettings.TenantId);
            HttpClient client = new()
            {
                BaseAddress = new Uri(appSettings.WebApiUrl),
                Timeout = new TimeSpan(0, 2, 0)  // 2 minutes
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return client;
        }        

        private static async Task<string> GetAccessToken(string baseUrl, string clientId, string clientSecret, string tenantId)
        {
            string[] scopes = { baseUrl + "/.default" };
            string authority = $"https://login.microsoftonline.com/{tenantId}";

            var clientApp = ConfidentialClientApplicationBuilder.Create(clientId: clientId)
                                                      .WithClientSecret(clientSecret: clientSecret)
                                                      .WithAuthority(new Uri(authority))
                                                      .Build();

            var builder = clientApp.AcquireTokenForClient(scopes);
            var result = await builder.ExecuteAsync();

            return result.AccessToken;         
        }
    }
}
