using static System.Net.Mime.MediaTypeNames;

namespace ECC.Institute.CRM.IntegrationAPI
{

    public class ApplicationFactory
    {
        public static Application Create(ID365WebAPIService d365WebAPIService, string name)
        {
            var appSettings = d365WebAPIService.D365AuthenticationService.D365AppSettings.FirstOrDefault(a => a.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (appSettings == null)
            {
                throw new Exception($"Application {name} is not supported.");
            }
            d365WebAPIService.Application = name;
            return new Application(d365WebAPIService);
        }
    }

    /// <summary>
    /// Notes:  Investigate if Upsert canbe implemented using the apprach discussed in the link below.  Alternate keys must be set for the tables for this to work
    /// Otherwise check for existing records in CRM, then create or update and based on the outcome.
    /// https://learn.microsoft.com/en-us/power-apps/developer/data-platform/use-upsert-insert-update-record?tabs=webapi 
    /// </summary>
    public class Application
    {
        private readonly ID365WebAPIService _d365webapiservice;

        public Application(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        public HttpResponseMessage DistrictUpsert(dynamic value)
        {
            throw new NotImplementedException();
        }

        public HttpResponseMessage AuthorityUpsert(string applicationName, dynamic value)
        {
            throw new NotImplementedException();
        }

        public HttpResponseMessage SchoolUpsert(dynamic value)
        {
            throw new NotImplementedException();
        }
    }

}
