using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ECC.Institute.CRM.IntegrationAPI.Controllers
{
    [Route("api/health-check")]
    [ApiController]
    public class HealthCheck : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;

		public HealthCheck(ID365WebAPIService d365webapiservice)
		{
            _d365webapiservice = d365webapiservice;
        }

        [HttpGet("")]
        public ActionResult<string> checkHealth() {
            // TODO: Replace the below with a proper API call for health check
            //var app = ApplicationFactory.Create(_d365webapiservice, "IOSAS");
            //string selectStatement = "contacts?$select=fullname,emailaddress1,contactid,firstname,lastname,telephone1,iosas_loginenabled,iosas_externaluserid,iosas_invitecode";
            //var respContactDetails = _d365webapiservice.SendCreateRequestAsyncRtn(selectStatement, "");
            return Ok("System Status: Healthy");
        }
    }
}

