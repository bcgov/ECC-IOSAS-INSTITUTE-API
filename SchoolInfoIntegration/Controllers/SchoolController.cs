using ECC.Institute.CRM.IntegrationAPI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

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
        public ActionResult<string> AuthorityUpsert(string applicationName, [FromBody] dynamic value)
        {
            try
            {
                _logger.LogInformation("Received request to update authoroties for :" + applicationName);
                var app = ApplicationFactory.Create(_d365webapiservice, applicationName);
                //var response = app.AuthorityUpsert(applicationName, value);

                var isAuthortyUpserted = _authorityService.upsert(applicationName, value);

                if (isAuthortyUpserted)
                {
                    return Ok($"To Do");
                }
                else {

                    // Handle falase response
                    throw new NotImplementedException();
                }
                return Ok($"To Do");
                //else
                //{
                //    return StatusCode((int)response.StatusCode, "Failed to upsert.");
                //}
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }            
        }

        [HttpPost("{application}/DistrictUpsert")]
        public ActionResult<string> DistrictUpsert(string application, [FromBody] dynamic value)
        {
            try
            {
                var app = ApplicationFactory.Create(_d365webapiservice, application);
                var response = app.DistrictUpsert(value);
                if (response.IsSuccessStatusCode)
                {
                    return Ok($"To Do");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to upsert.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{application}/SchoolUpsert")]
        public ActionResult<string> SchoolUpsert(string application, [FromBody] dynamic value)
        {
            try
            {
                // TODO: Add log for the request 
                var app = ApplicationFactory.Create(_d365webapiservice, application);
                var response = app.SchoolUpsert(value);
                if (response.IsSuccessStatusCode)
                {
                    return Ok($"To Do");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to upsert.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
