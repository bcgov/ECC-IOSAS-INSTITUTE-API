using IOSAS.Infrastructure.WebAPI.Models;

namespace IOSAS.Infrastructure.WebAPI.Services
{
    public interface ID365AuthenticationService
    {
        Task<HttpClient> GetHttpClient();
        Task<HttpClient> GetHttpClient(bool isSearch);
        public D365AppSettings D365AppSettings { get; }
    }
}