using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Xml.Linq;
using ECC.Institute.CRM.IntegrationAPI;
//using IOSAS.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json.Linq;
using ECC.Institute.CRM.IntegrationAPI.Model;
using ECC.Institute.CRM.IntegrationAPI.Filters;

namespace ECC.Institute.CRM.IntegrationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public class MetdataController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        private readonly ILogger<MetdataController> _logger;
        private string JSONResp(string input) => $"{JObject.Parse(input)}";
        public MetdataController(ID365WebAPIService d365webapiservice, ILogger<MetdataController> logger)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
            _logger = logger;
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
                    return Ok(JSONResp(response.Content.ReadAsStringAsync().Result));
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


        [HttpGet("GetAllChoiceValues/{applicationName}")]
        public ActionResult<string> GetAllChoiceValues([FromRoute] string applicationName, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");
            if (string.IsNullOrEmpty(applicationName))
                return BadRequest("Invalid Request - applicationName is required");
            _d365webapiservice.Application = applicationName;
            // GetMultiSelectPicklistValues
            string message = $"EntityDefinitions(LogicalName='{tableName}')/Attributes/Microsoft.Dynamics.CRM.MultiSelectPicklistAttributeMetadata?$select=LogicalName&$expand=GlobalOptionSet($select=Options)";
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            JObject[] multiSelecList;
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().First().HasValues)
                {
                    var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    multiSelecList = MinimalFieldDescription(result, tableName);
                }
                else
                {
                    multiSelecList = Array.Empty<JObject>();
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");

            // PickList
            JObject[] pickList;
            string messagePickList = $"EntityDefinitions(LogicalName='{tableName}')/Attributes/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName&$expand=GlobalOptionSet($select=Options)";
            var response2 = _d365webapiservice.SendMessageAsync(HttpMethod.Get, messagePickList);
            if (response2.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response2.Content.ReadAsStringAsync().Result);
                System.Console.WriteLine($"Response: {root}");
                if (root.Last().First().HasValues)
                {
                    var result = JObject.Parse(response2.Content.ReadAsStringAsync().Result);
                    pickList = MinimalFieldDescription(result, tableName);
                }
                else
                {
                    pickList = Array.Empty<JObject>();
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");

            JObject resp = new();
            resp["tablename"] = tableName;
            resp["application"] = applicationName;
            resp["choiceFields"] = JToken.FromObject(pickList.Concat(multiSelecList).ToArray());
            return Ok($"{resp}");
        }


        [HttpGet("GetMultiSelectPicklistValues/{applicationName}")]
        public ActionResult<string> GetMultiSelectPicklistValues([FromRoute] string applicationName, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");
            if (string.IsNullOrEmpty(applicationName))
                return BadRequest("Invalid Request - applicationName is required");
            _d365webapiservice.Application = applicationName;
            string message = $"EntityDefinitions(LogicalName='{tableName}')/Attributes/Microsoft.Dynamics.CRM.MultiSelectPicklistAttributeMetadata?$select=LogicalName&$expand=GlobalOptionSet($select=Options)";
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().First().HasValues)
                {
                    var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    return Ok(JSONResp(response.Content.ReadAsStringAsync().Result));
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

        [HttpGet("GetPickListValues/{applicationName}/{tableName}")]
        public ActionResult<string> GetPickListValues([FromRoute] string applicationName,[FromRoute] string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");
            if (string.IsNullOrEmpty(applicationName))
                return BadRequest("Invalid Request - applicationName is required");
            _d365webapiservice.Application = applicationName;
            string message = $"EntityDefinitions(LogicalName='{tableName}')/Attributes/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName&$expand=GlobalOptionSet($select=Options)";
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                System.Console.WriteLine($"Response: {root}");
                if (root.Last().First().HasValues)
                {
                    var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    var finalResponse = MinimalFieldDescription(result, tableName);
                    return Ok($"{finalResponse}");
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

        private JObject[] MinimalFieldDescription(JObject data, string tableName)
        {
            JObject[] values = (data.GetValue("value")?.ToArray() ?? Array.Empty<JObject>())
                                .Select(item => (JObject)item).ToArray();
            List<JObject> items = new();
            if (values.Length > 0)
            {
                foreach(JObject des in values)
                {
                    JObject item = new();
                    item["field_name"] = des["LogicalName"] ?? "NA";
                    JObject optionSet = (JObject)(des["GlobalOptionSet"] ?? new JObject());
                    JObject[] options = (optionSet.GetValue("Options")?.ToArray() ?? Array.Empty<JObject>())
                                .Select(opt => (JObject)opt).ToArray();
                    if (options.Length > 0)
                    {
                        List<JObject> fieldOtionValues = new();
                        foreach(JObject option in options)
                        {
                            JObject fieldOption = new();
                            fieldOption["value"] = option["Value"] ?? "NA";
                            JObject lable = option.GetValue("Label") as JObject ?? new();
                            JObject[] localized = (lable.GetValue("LocalizedLabels")?.ToArray() ?? Array.Empty<JObject>())
                                .Select(opt => (JObject)opt).ToArray();
                            fieldOption["lable"] = localized[0].GetValue("Label") ?? "NA";
                            fieldOtionValues.Add(fieldOption);
                        }
                        item["options"] = JToken.FromObject(fieldOtionValues.ToArray());
                    } else
                    {
                        item["options"] = JToken.FromObject(Array.Empty<JObject>());
                        item["remarks"] = "No option values";
                    }
                    items.Add(item);
                }
                return items.ToArray();
            }
            return Array.Empty<JObject>();
        }

        [HttpGet("GetFieldDescritions")]
        public ActionResult<string> GetFieldDescritions(string applicationName,string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");

            if (string.IsNullOrEmpty(applicationName))
                return BadRequest("Invalid Request - applicationName is required");

            string message = string.Format($"EntityDefinitions(LogicalName='{tableName}')?$select=LogicalName&$expand=Attributes($select=LogicalName,DisplayName,Description)");

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().HasValues)
                {
                    return Ok(JSONResp(response.Content.ReadAsStringAsync().Result));
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
        public ActionResult<string> GetMetadataDefinition(string applicationName, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");

            if (string.IsNullOrEmpty(applicationName))
                return BadRequest("Invalid Request - tableName is required");

            string message = string.Format($"EntityDefinitions(LogicalName='{tableName}')?$select=LogicalName&$expand=Attributes");

            _d365webapiservice.Application = applicationName;
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                System.Console.WriteLine($"[{tableName}] Response: {root}");
                if (root.Last().HasValues)
                {
                    return Ok(JSONResp(response.Content.ReadAsStringAsync().Result));
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
        [HttpGet("GetSelectedData")]
        public ActionResult<string> GetSelectedData(string applicationName, string tableName, string selectQuery)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");

            if (string.IsNullOrEmpty(applicationName))
                return BadRequest("Invalid Request - tableName is required");

            string message = string.Format($"{tableName}?$select={selectQuery}");

            _d365webapiservice.Application = applicationName;
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                System.Console.WriteLine($"[{tableName}] Response: {root}");
                if (root.Last().HasValues)
                {
                    return Ok(JSONResp(response.Content.ReadAsStringAsync().Result));
                }
                else
                {
                    return NotFound($"No Data");
                }
            }
            else
                System.Console.WriteLine($"Response error: {response.StatusCode}, {response.ReasonPhrase} | {response.Content.ReadAsStringAsync().Result} | URI: {response.RequestMessage?.RequestUri} ");
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }

        [HttpGet("GetFilteredData")]
        public ActionResult<string> GetFilteredData(string applicationName, string tableName, string columns, string keyName, string values, Boolean isNumber)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");

            if (string.IsNullOrEmpty(applicationName))
                return BadRequest("Invalid Request - tableName is required");

            if (string.IsNullOrEmpty(keyName))
                return BadRequest("Invalid Request - select is required");

            if (string.IsNullOrEmpty(columns))
                return BadRequest("Invalid Request - colmuns is required");

            if (string.IsNullOrEmpty(values))
                return BadRequest("Invalid Request - values is required");
            var fianlValues = values.Split(",").Select(val => val == "'null'" ? "null" : $"'{val}'"); // isNumber ? $"{val}" : $"\"{val}\"")
            string message = string.Format($"{tableName}?$select={columns}&$filter=Microsoft.Dynamics.CRM.In(PropertyName='{keyName}',PropertyValues=[{string.Join(",", fianlValues)}])");

            _logger.LogInformation($"The Filter Query: {message} | Application: {applicationName}");

            _d365webapiservice.Application = applicationName;
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                System.Console.WriteLine($"[{tableName}] Response: {root}");
                if (root.Last().HasValues)
                {
                    return Ok(JSONResp(response.Content.ReadAsStringAsync().Result));
                }
                else
                {
                    return NotFound($"No Data");
                }
            }
            else
                System.Console.WriteLine($"Response error: {response.StatusCode}, {response.ReasonPhrase} | {response.Content.ReadAsStringAsync().Result} | URI: {response.RequestMessage?.RequestUri} ");
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