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
        [ServiceFilter(typeof (ValidationFilterAttribute))]
        public ActionResult<string> AuthorityUpsert(string applicationName, [FromBody] SchoolAuthority[] authorities)
        {
            try
            {
                _logger.LogInformation("Received request to update authoroties for :" + applicationName);
                foreach (SchoolAuthority authority in authorities)
                {
                    _logger.LogInformation("Received authority number = " + authority.AuthorityNumber + ", id = " + authority.IndependentAuthorityId);
                }
                return Ok($"OK");
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }            
        }

        [HttpPost("{applicationName}/DistrictUpsert")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public ActionResult<string> DistrictUpsert(string applicationName, [FromBody] SchoolDistrict[] districts)
        {
            try
            {
                _logger.LogInformation("Received request to update authoroties for :" + applicationName);
                foreach (SchoolDistrict district in districts)
                {
                    _logger.LogInformation("Received district number = " + district.DistrictNumber + ", name = " + district.DisplayName);
                }
                return Ok($"OK");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{applicationName}/SchoolUpsert")]
        public ActionResult<string> SchoolUpsert(string applicationName, [FromBody] School[] schools)
        {
            try
            {
                _logger.LogInformation("Received request to update authoroties for :" + applicationName);
                foreach (School school in schools)
                {
                    _logger.LogInformation("Received school number = " + school.Mincode + ", name = " + school.DisplayName);
                }
                return Ok($"OK");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
