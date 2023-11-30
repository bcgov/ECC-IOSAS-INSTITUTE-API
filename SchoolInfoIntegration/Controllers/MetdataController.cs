﻿using System;
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

        [HttpGet("GetPickListValues/{applicationName}/{tableName}")]
        public ActionResult<string> GetPickListValues([FromRoute] string applicationName,[FromRoute] string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");
            _d365webapiservice.Application = applicationName;
            string message = $"EntityDefinitions(LogicalName='{tableName}')/Attributes/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName&$expand=GlobalOptionSet($select=Options)";
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                System.Console.WriteLine($"Response: {root}");
                if (root.Last().First().HasValues)
                {
                    return Ok(response.Content.ReadAsStringAsync().Result.Replace($"/", ""));
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
                    return Ok(response.Content.ReadAsStringAsync().Result);
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
        public ActionResult<string> GetFilteredData(string applicationName, string tableName, string selectQuery, string filterQuery)
        {
            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Invalid Request - tableName is required");

            if (string.IsNullOrEmpty(applicationName))
                return BadRequest("Invalid Request - tableName is required");

            if (string.IsNullOrEmpty(selectQuery))
                return BadRequest("Invalid Request - select is required");

            if (string.IsNullOrEmpty(filterQuery))
                return BadRequest("Invalid Request - filter is required");

            string message = string.Format($"{tableName}?$select={selectQuery}&$filter={filterQuery}");

            _d365webapiservice.Application = applicationName;
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                System.Console.WriteLine($"[{tableName}] Response: {root}");
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