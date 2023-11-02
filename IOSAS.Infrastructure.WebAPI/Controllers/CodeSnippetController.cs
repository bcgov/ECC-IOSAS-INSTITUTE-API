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
    public class CodeSnippetController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public CodeSnippetController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

 
        [HttpGet("GetCodeSnippets")]
        public ActionResult<string> GetCodeSnippets()
        {

            var fetchXml = @"<fetch version='1.0' mapping='logical' distinct='true'>
                                <entity name='iosas_codesnippets'>
                                    <attribute name='iosas_name'/>
                                    <order attribute='iosas_name' descending='false'/>
                                    <attribute name='iosas_value'/>
                                    <attribute name='statuscode'/>
                                    <filter type='or'>
                                        <condition attribute='statecode' operator='eq' value='0'/>
                                    </filter>
                                </entity>
                            </fetch>";

            var message = $"iosas_codesnippetses?fetchXml=" + WebUtility.UrlEncode(fetchXml);

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