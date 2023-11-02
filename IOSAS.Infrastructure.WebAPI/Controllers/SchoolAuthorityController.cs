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
    public class SchoolAuthorityController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public SchoolAuthorityController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }


        [HttpGet("GetActiveSchoolAuthorityList")]
        public ActionResult<string> GetActiveSchoolAuthorityList(string? authorityName=null)
        {
            var conditions = string.IsNullOrEmpty(authorityName) ? 
                "<filter type='and'><condition attribute='statecode' operator='eq' value='0'/></filter>" :
                $"<filter type='and'>" +
                $"<condition attribute='statecode' operator='eq' value='0'/>" +
                $"<condition attribute='edu_authority_type' operator='eq' value='757500000'/>" +
                $"<condition attribute='edu_name' operator='like' value='%{authorityName}%'/>" +
                $"</filter>";

     
            var fetchXml = $@"<fetch version='1.0' mapping='logical' distinct='true'>
                                <entity name='edu_schoolauthority'>
                                    <attribute name='edu_name'/>
                                    <attribute name='edu_schoolauthorityid'/>
                                    <attribute name='edu_vendorlocationcode'/>
                                    <attribute name='edu_fax'/>
                                    <attribute name='edu_email'/>
                                    <attribute name='edu_address_street1'/>
                                    <attribute name='edu_address_street2'/>
                                    <attribute name='edu_address_city'/>
                                    <attribute name='edu_address_province'/>
                                    <attribute name='edu_address_country'/>
                                    <attribute name='edu_address_postalcode'/>
                                     {conditions}
                                    <order attribute='edu_name' descending='false'/>
                                </entity>
                            </fetch>";

            var message = $"edu_schoolauthorities?fetchXml=" + WebUtility.UrlEncode(fetchXml);

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