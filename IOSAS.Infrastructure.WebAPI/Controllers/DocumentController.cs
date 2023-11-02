using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using IOSAS.Infrastructure.WebAPI.Models;
using IOSAS.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace IOSAS.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;

        public DocumentController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));

        }

        [HttpPost("Upload")]
        public ActionResult<string> Upload([FromBody] dynamic document)
        {
            var rawDocument = document.ToString();
            Document doc = JsonConvert.DeserializeObject<Document>(rawDocument);

            if (doc.Content == null )
                return BadRequest("Content is not provided");

            if (doc.DocumentType == null || !doc.DocumentType.HasValue)
                return BadRequest("DocumentType is not provided");

            if (doc.DocumentName == null)
                return BadRequest("DocumentName is not provided");

            if (doc.FileName == null)
                return BadRequest("FileName is not provided");

            if (doc.RegardingId == null)
                return BadRequest("RegardingId is not provided");

            if (doc.RegardingType == null)
                return BadRequest("RegardingType is not provided");


            //check for allowed object types
            string[] acceptedObjectTypes = { "iosas_application", "iosas_expressionofinterest" };
            if (Array.IndexOf(acceptedObjectTypes, doc.RegardingType) == -1)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Invalid RegardingType, allowed values are: iosas_application,iosas_expressionofinterest");
            }

            //check for allowed file formats
            string[] partialfilename = doc.FileName.Split('.');
            string ext = partialfilename[partialfilename.Count() - 1].ToLower();

            //stop, if the file format is not valid
            string[] acceptedFileFormats = _d365webapiservice.D365AuthenticationService.D365AppSettings.AllowedFileUplaodTypes.Split(",");
            if (Array.IndexOf(acceptedFileFormats, ext) == -1)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Uploaded file format is not supported, valif formats are: {_d365webapiservice.D365AuthenticationService.D365AppSettings.AllowedFileUplaodTypes}.");
            }

            long? maxFileSize = GetMaxFileSize("iosas_document", "iosas_file");
            if (doc.Content.Length > maxFileSize)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"The file size exceeds the limit allowed ({maxFileSize}).");
            }

            //Create Document Record
            JObject docJson = new JObject();
            docJson["iosas_name"] = doc.DocumentName;
            if (doc.RegardingType.ToLower().Equals("iosas_application"))
                docJson["iosas_newschoolapplicationdocumenttype"] = doc.DocumentType;
            else
                docJson["iosas_eoidocumenttype"] = doc.DocumentType;

            if (doc.DocumentCategory.HasValue && doc.DocumentCategory > 0 && doc.RegardingType.ToLower().Equals("iosas_application"))
            {
                docJson["iosas_documentcategory"] = doc.DocumentCategory;
            }

            //fieldName_tableName
            string key = doc.RegardingType.ToLower().Equals("iosas_application") ? "iosas_RegardingId_iosas_application@odata.bind" : "iosas_RegardingId_iosas_expressionofinterest@odata.bind";
            docJson[key] = $"/{doc.RegardingType}s({doc.RegardingId})";


            var createResp = _d365webapiservice.SendCreateRequestAsync("iosas_documents", docJson.ToString());
            if (createResp.IsSuccessStatusCode)
            {
                var entityUri = createResp.Headers.GetValues("OData-EntityId").First();

                string pattern = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
                Match m = Regex.Match(entityUri, pattern, RegexOptions.IgnoreCase);
                var id = string.Empty;
                if (m.Success)
                {
                    id = m.Value;

                    var bytes = Convert.FromBase64String(doc.Content);
                    var content = new MemoryStream(bytes);
                    var response = _d365webapiservice.SendUploadFileRequestAsync($"iosas_documents({id})/iosas_file?x-ms-file-name={doc.FileName}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return Ok("The document uploaded successfully.");
                    }
                    else
                        return StatusCode((int)response.StatusCode,
                            $"Failed to upload the document: {response.ReasonPhrase}");
                }
                else
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to create record at this time");
            }
            else
                return StatusCode((int)createResp.StatusCode,
                    $"Failed to create record: {createResp.ReasonPhrase}");

        }

        [HttpDelete("Remove")]
        public ActionResult<string> Remove(string id)
        {
            id = "iosas_documents(" + id + ")";
            var response = _d365webapiservice.SendDeleteRequestAsync(id);
            if (response.IsSuccessStatusCode)
            {
                return Ok("The document has been removed");
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to remove the document: {response.ReasonPhrase}");
        }


        [HttpGet("GetApplicationDocumentList")]
        public ActionResult<string> GetApplicationDocumentList(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("Invalid Request");

            var fetchXml = $@"<fetch>
  <entity name='iosas_document'>
    <attribute name='createdby' />
    <attribute name='createdon' />
    <attribute name='iosas_documentid' />
    <attribute name='iosas_newschoolapplicationdocumenttype' />
    <attribute name='iosas_documentcategory' />
    <attribute name='iosas_file' />
    <attribute name='iosas_name' />
    <attribute name='iosas_regardingid' />
    <attribute name='modifiedby' />
    <attribute name='modifiedon' />
    <attribute name='statecode' />
    <attribute name='statuscode' />
    <filter>
      <condition attribute='iosas_regardingid' operator='eq' value='{id}' />
    </filter>
  </entity>
</fetch>";

            var message = $"iosas_documents?fetchXml=" + WebUtility.UrlEncode(fetchXml);
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

        [HttpGet("GetEOIDocumentList")]
        public ActionResult<string> GetEOIDocumentList(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("Invalid Request");

            var fetchXml = $@"<fetch>
  <entity name='iosas_document'>
    <attribute name='createdby' />
    <attribute name='createdon' />
    <attribute name='iosas_documentid' />
    <attribute name='iosas_eoidocumenttype' />
    <attribute name='iosas_file' />
    <attribute name='iosas_name' />
    <attribute name='iosas_regardingid' />
    <attribute name='modifiedby' />
    <attribute name='modifiedon' />
    <attribute name='statecode' />
    <attribute name='statuscode' />
    <filter>
      <condition attribute='iosas_regardingid' operator='eq' value='{id}' />
    </filter>
  </entity>
</fetch>";

            var message = $"iosas_documents?fetchXml=" + WebUtility.UrlEncode(fetchXml);
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

        private int GetMaxFileSize(string tableName, string fieldName)
        {
            var result = 32 * 1024* 1024;

            string message = new($"EntityDefinitions(LogicalName='{tableName}')" +
                                  $"/Attributes(LogicalName='{fieldName}')" +
                                  $"/Microsoft.Dynamics.CRM.FileAttributeMetadata?$select=MaxSizeInKB");

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().HasValues)
                {
                    result = root["MaxSizeInKB"] == null ? result : (int)root["MaxSizeInKB"]*1024;
                }
                
            }
           return result;
        }
    }
}