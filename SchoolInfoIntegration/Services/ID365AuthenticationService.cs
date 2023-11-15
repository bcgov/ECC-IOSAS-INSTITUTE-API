using ECC.Institute.CRM.IntegrationAPI.Models;

namespace ECC.Institute.CRM.IntegrationAPI
{
    public interface ID365AuthenticationService
    {
        Task<HttpClient> GetHttpClient(string applicationName);     
        public List<D365AppSettings> D365AppSettings { get; }
    }
}