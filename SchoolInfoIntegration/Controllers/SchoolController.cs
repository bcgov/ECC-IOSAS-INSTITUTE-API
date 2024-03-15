using ECC.Institute.CRM.IntegrationAPI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using ECC.Institute.CRM.IntegrationAPI.Model;
using ECC.Institute.CRM.IntegrationAPI.Filters;
using CsvHelper;
using System.Globalization;
using System.Text;
namespace ECC.Institute.CRM.IntegrationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public class SchoolController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        private readonly ILogger<SchoolController> _logger;
        private readonly IAuthoritiesService _authorityService;
        static ImpoterService? iosasServic;
        static ImpoterService? isfsService;

        public SchoolController(ID365WebAPIService d365webapiservice, ILogger<SchoolController> logger, IAuthoritiesService authoritiesService)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
            _logger = logger;
            _authorityService = authoritiesService;
            //_iosasService = ImporterFactory.Create(d365webapiservice, "iosas", _logger);
            //_isfsService = ImporterFactory.Create(d365webapiservice, "isfs", _logger);
        }

        private ImpoterService getService(string applicationName)
        {
            if (SchoolController.isfsService == null)
            {
                SchoolController.isfsService = ImporterFactory.Create(_d365webapiservice, "isfs", _logger);
            }
            if (SchoolController.iosasServic == null)
            {
                SchoolController.iosasServic = ImporterFactory.Create(_d365webapiservice, "iosas", _logger);
            }
            return applicationName == "isfs" ? SchoolController.isfsService : SchoolController.iosasServic;
        }

        [HttpPost("{applicationName}/AuthorityUpsert")]
        public ActionResult<string> AuthorityUpsert([FromRoute] string applicationName, [FromBody] SchoolAuthority[] authorities)
        {
            try
            {
                _logger.LogInformation($"Received request to update authoroties for : {applicationName}");
                D365Application application = D365Application.FromString(applicationName.ToLower());
                var app = ApplicationFactory.Create(_d365webapiservice, applicationName.ToUpper(), _logger);
                if (application == D365Application.IOSAS)
                {
                    return Ok(app.AuthorityUpsertIOSAS(authorities));
                } else if (application == D365Application.ISFS)
                {
                    return Ok(app.AuthorityUpsertISFS(authorities));
                } else
                {
                    return StatusCode(422, $"Unable to process application with name: {applicationName}");
                }
                
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }            
        }

        [HttpPost("{applicationName}/DistrictUpsert")]
        public ActionResult<string> DistrictUpsert([FromRoute] string applicationName, [FromBody] SchoolDistrict[] districts)
        {
            try
            {
                _logger.LogInformation($"Received request to update authoroties for : {applicationName}");
                foreach (SchoolDistrict district in districts)
                {
                    _logger.LogInformation($"Received district number = {district.DistrictNumber}, name = {district.DisplayName}");
                }
                D365Application application = D365Application.FromString(applicationName.ToLower());
                var app = ApplicationFactory.Create(_d365webapiservice, applicationName.ToUpper(), _logger);
                if (application == D365Application.IOSAS)
                {
                    return Ok(app.DistrictUpsertIOSAS(districts));
                }
                else if (application == D365Application.ISFS)
                {
                    return Ok(app.DistrictUpsertISFS(districts));
                }
                else
                {
                    return StatusCode(422, $"Unable to process application with name: {applicationName}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{applicationName}/SchoolUpsert")]
        public ActionResult<string> SchoolUpsert([FromRoute] string applicationName, [FromBody] School[] schools)
        {
            try
            {
                _logger.LogInformation($"Received request to update authoroties for : {applicationName}");
                foreach (School school in schools)
                {
                    _logger.LogInformation($"Received school number = {school.Mincode} name = {school.DisplayName}");
                }
                D365Application application = D365Application.FromString(applicationName.ToLower());
                var app = ApplicationFactory.Create(_d365webapiservice, applicationName.ToUpper(), _logger);
                if (application == D365Application.IOSAS)
                {
                    return Ok(app.SchoolUpsertIOSAS(schools));
                }
                else if (application == D365Application.ISFS)
                {
                    return Ok(app.SchoolUpsertForISFS(schools));
                }
                else
                {
                    return StatusCode(422, $"Unable to process application with name: {applicationName}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("/history")]
        public ActionResult<string> GetHistory()
        {
            var history = new List<JObject>(Application.GetHistory());
            history.Reverse();
            return Ok($"{JToken.FromObject(history)}");
        }

        [HttpPost("/import/{applicationName}/{entityName}")]
        public ActionResult<string> Import([FromRoute] string applicationName, [FromRoute] string entityName, IFormFile file, Boolean isVerifyOnly = true)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Upload a csv file");
                }
                using (var stream = new StreamReader(file.OpenReadStream()))
                using(var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
                {
                    ImpoterService service = getService(applicationName);
                    if (service.isRunning)
                    {
                        return Ok(service.TaskStatus());
                    }
                    return Ok(service.ExternalIdImport(applicationName, entityName, csv, isVerifyOnly));
                }
                    
            } catch (Exception excp)
            {
                return StatusCode(500, excp.Message);
            }
        }

        [HttpGet("/import/{applicationName}/status")]
        public ActionResult<string> ImportStatus([FromRoute] string applicationName)
        {
            return Ok(getService(applicationName).TaskStatus());
        }
        [HttpGet("/import/{applicationName}/report")]
        public IActionResult ImportReport([FromRoute] string applicationName)
        {
            string report = getService(applicationName).ImportReport();
            return GetCSVFile(report, $"status-report-{applicationName}");
        }
        [HttpPost("/verify/{applicationName}/{entityName}")]
        public IActionResult Verify([FromRoute] string applicationName, [FromRoute] string entityName, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Upload a csv file");
                }
                using (var stream = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
                {
                    ImpoterService service = getService(applicationName);
                    if (service.isRunning)
                    {
                        return Ok(service.TaskStatus());
                    }
                    string report = service.Verify(applicationName, entityName, csv);
                    return GetCSVFile(report, $"report-{applicationName}-{entityName}");
                }

            }
            catch (Exception excp)
            {
                return StatusCode(500, excp.Message);
            }
        }
        private IActionResult GetCSVFile(string content, string name)
        {
            var byteArray = Encoding.ASCII.GetBytes(content);
            return File(byteArray, "text/csv", $"{name}.csv");
        }
    }
}
