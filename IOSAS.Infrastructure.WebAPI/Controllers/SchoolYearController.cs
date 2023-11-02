using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using IOSAS.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace IOSAS.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolYearController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public SchoolYearController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        [HttpGet("GetById")]
        public ActionResult<string> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Invalid Request - Id is required");

          
            var fetchXml = $@"<fetch version='1.0' mapping='logical' distinct='true'>
                                <entity name='edu_year'>
                                    <attribute name='edu_name'/>
                                    <order attribute='edu_name' descending='false'/>
                                    <attribute name='edu_type'/>
                                    <attribute name='statuscode'/>
                                    <attribute name='edu_startdate'/>
                                    <attribute name='edu_enddate'/>
                                    <attribute name='edu_yearid'/>
                                    <attribute name='iosas_label'/>
                                    <attribute name='iosas_interviewdeadline'/>
                                    <attribute name='iosas_precertificationsubmissiondeadline'/>
                                    <attribute name='iosas_currentapplicationsyear'/>
                                    <filter type='and'>
                                        <condition attribute='edu_yearid' operator='eq' value='{id}'/>
                                    </filter>
                                </entity>
                            </fetch>";

            var message = $"edu_years?fetchXml=" + WebUtility.UrlEncode(fetchXml);

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().First().HasValues)
                {
                    return Ok(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    return NotFound($"No Data: {id}");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }

        [HttpGet("GetActiveYears")]
        public ActionResult<string> GetActiveYears()
        {

            var fetchXml = $@"<fetch version='1.0' mapping='logical' distinct='true'>
                                <entity name='edu_year'>
                                    <attribute name='edu_name'/>
                                    <order attribute='edu_name' descending='false'/>
                                    <attribute name='edu_type'/>
                                    <attribute name='statuscode'/>
                                    <attribute name='edu_startdate'/>
                                    <attribute name='edu_enddate'/>
                                    <attribute name='edu_yearid'/>
                                    <attribute name='iosas_label'/> 
                                    <attribute name='iosas_currentapplicationsyear'/>
                                    <filter type='and'>
                                        <condition attribute='statecode' operator='eq' value='0'/>
                                     </filter>
                                </entity>
                            </fetch>";

            var message = $"edu_years?fetchXml=" + WebUtility.UrlEncode(fetchXml);

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().First().HasValues)
                {
                    return Ok(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    return Ok($"[]");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }

    }
}