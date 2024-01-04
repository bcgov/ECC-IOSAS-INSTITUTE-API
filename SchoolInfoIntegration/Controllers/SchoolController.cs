using ECC.Institute.CRM.IntegrationAPI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using ECC.Institute.CRM.IntegrationAPI.Model;
using ECC.Institute.CRM.IntegrationAPI.Filters;

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

        public SchoolController(ID365WebAPIService d365webapiservice, ILogger<SchoolController> logger, IAuthoritiesService authoritiesService)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
            _logger = logger;
            _authorityService = authoritiesService;

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
                var app = ApplicationFactory.Create(_d365webapiservice, applicationName.ToUpper(), _logger);
                return Ok(app.SchoolUpsert(schools));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("lookup/{applicationName}/{entityName}")]
        public ActionResult<string> LookUp([FromRoute] string applicationName, [FromRoute] string entityName)
        {
            try
            {
                return Ok("OK");
            } catch (Exception excp)
            {
                return StatusCode(500, excp.Message);
            }
        }
    }
}
