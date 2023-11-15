using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Xml.Linq;
using IOSAS.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json.Linq;

namespace IOSAS.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetdataController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public MetdataController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }


        [HttpGet("GetFieldPickListValues")]
        public ActionResult<string> GetFieldPickListValues(string tableName, string fieldName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");

            if (string.IsNullOrEmpty(fieldName))
                return BadRequest("Invalid Request - fieldName is required");

            var message = GenerateUri(tableName, fieldName, "PicklistAttributeMetadata", "?$select=SchemaName&$expand=OptionSet,GlobalOptionSet");

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
                    return NotFound($"No Data");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }


        [HttpGet("GetMultiSelectPicklistValues")]
        public ActionResult<string> GetMultiSelectPicklistValues(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");

            string message = $"EntityDefinitions(LogicalName='{tableName}')/Attributes/Microsoft.Dynamics.CRM.MultiSelectPicklistAttributeMetadata?$select=LogicalName&$expand=GlobalOptionSet($select=Options)";
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
                    return NotFound($"No Data");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }

        [HttpGet("GetPickListValues")]
        public ActionResult<string> GetPickListValues(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");

            string message = $"EntityDefinitions(LogicalName='{tableName}')/Attributes/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName&$expand=GlobalOptionSet($select=Options)";
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
                    return NotFound($"No Data");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }

        [HttpGet("GetFieldDescritions")]
        public ActionResult<string> GetFieldDescritions(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");

           string message = string.Format($"EntityDefinitions(LogicalName='{tableName}')?$select=LogicalName&$expand=Attributes($select=LogicalName,DisplayName,Description)");

           var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().HasValues)
                {
                    return Ok(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    return NotFound($"No Data");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }

        [HttpGet("GetMetadataDefinition")]
        public ActionResult<string> GetMetadataDefinition(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");

            string message = string.Format($"EntityDefinitions(LogicalName='{tableName}')?$select=LogicalName&$expand=Attributes");

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().HasValues)
                {
                    return Ok(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    return NotFound($"No Data");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }
   


        private string GenerateUri(string tableName, string fieldName, string fieldType, string query)
        {
            string baseUri = new($"EntityDefinitions(LogicalName='{tableName}')" +
                                    $"/Attributes(LogicalName='{fieldName}')" +
                                    $"/Microsoft.Dynamics.CRM.{fieldType}");
            if (query != null)
            {

                baseUri += query;
            }

            return baseUri;

        }
    }
}