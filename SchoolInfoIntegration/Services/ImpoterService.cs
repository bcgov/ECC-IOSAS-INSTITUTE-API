using System;
using ECC.Institute.CRM.IntegrationAPI.Model;
using Newtonsoft.Json.Linq;
using CsvHelper.Configuration;
namespace ECC.Institute.CRM.IntegrationAPI.Services
{
    public class ImporterFactory
    {
        public static ImpoterService Create(ID365WebAPIService d365WebAPIService, string name, ILogger logger)
        {
            var appSettings = d365WebAPIService.D365AuthenticationService.D365AppSettings.FirstOrDefault(a => a.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (appSettings == null)
            {
                throw new Exception($"ImpoterService: Application {name} is not supported.");
            }
            d365WebAPIService.Application = name;
            return new ImpoterService(d365WebAPIService, logger);
        }
    }
    public class ImportConfig
    {
        readonly string ConfigName;
        readonly D365ModelMetdaData Meta;
        readonly ClassMap Mapper;

        public ImportConfig(string name, D365ModelMetdaData meta, ClassMap mapper)
        {
            this.ConfigName = name;
            this.Meta = meta;
            this.Mapper = mapper;
        }
    }
    public class ImpoterService
    {
        private readonly ID365WebAPIService _d365webapiservice;
        private readonly ILogger _logger;
        private readonly Dictionary<string, ImportConfig> Config = new();
        private readonly Application Application;
        public ImpoterService(ID365WebAPIService d365webapiservice, ILogger logger)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
            _logger = logger;
            Application = new Application(d365webapiservice, logger);
            // 1. Authority Mapper
            ClassMap<SchoolAuthorityImport> mapper = new AuthorityMapper();
            ImportConfig schoolAuthorityIOSASConfig = new("schoolAuthorityIOSAS", new SchoolAuthorityIOSAS(), mapper);
            Config.Add(
                "schoolAuthority-iosas",
                schoolAuthorityIOSASConfig);
            ImportConfig schoolAuthorityISFSConfig = new("schoolAuthorityISFS", new SchoolAuthorityIOSAS(), mapper);
            Config.Add(
                "schoolAuthority-isfs",
                schoolAuthorityIOSASConfig);


        }
    }
}


