using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
using Microsoft.Xrm.Sdk.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace IOSAS.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public ContactController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        [HttpPost("Login")]
        public ActionResult<string> Login([FromBody] dynamic value)
        {

            //Validate fields from body and check if user is existing or new
            //Create if does not exist and send external Id back to user.
            //Returning crm contactid.
            //throw new NotImplementedException();

            if (value.iosas_externaluserid == null)
                return BadRequest("iosas_externaluserid is not provided.");

            if (value.emailaddress1 == null)
                return BadRequest("emailaddress1 is not provided");

            if (value.firstname == null)
                return BadRequest("firstname is not provided");

            if (value.lastname == null)
                return BadRequest("lastname is not provided");

            //if (value.telephone1 == null)
            //    return BadRequest("telephone1 is not provided");

            string fetchXml = "";
            //if (value.iosas_invitecode == null)
            //{
            //check by email or external id
            fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='false' distinct='true'>
                                    <entity name='contact'>
                                        <attribute name='entityimage_url' />
                                        <attribute name='fullname' />
                                        <attribute name='emailaddress1' />
                                        <attribute name='contactid' />
                                        <attribute name='firstname' />
                                        <attribute name='lastname' />
                                        <attribute name='telephone1' />
                                        <attribute name='iosas_loginenabled' />
                                        <attribute name='iosas_externaluserid' />
                                        <attribute name='iosas_invitecode' />
                                        <filter type='and'>
                                            <condition attribute='statecode' operator='eq' value='0' />
                                            <filter type='or'>
                                                <condition attribute='emailaddress1' operator='eq' value='{value.emailaddress1}' />
                                                <condition attribute='iosas_externaluserid' operator='eq' value='{value.iosas_externaluserid}' />
                                            </filter>
                                        </filter>
                                        <order attribute='fullname' descending='false' />
                                    </entity>
                                </fetch>";
            //}
            //else
            //{
            //    fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='false' distinct='true'>
            //                    <entity name='contact'>
            //                        <attribute name='fullname' />
            //                        <attribute name='emailaddress1' />
            //                        <attribute name='contactid' />
            //                        <attribute name='firstname' />
            //                        <attribute name='lastname' />
            //                        <attribute name='telephone1' />
            //                        <attribute name='iosas_loginenabled' />
            //                        <attribute name='iosas_externaluserid' />     
            //                        <filter type='and'>
            //                            <condition attribute='statecode' operator='eq' value='0' />
            //                            <filter type='or'>
            //                                <condition attribute='iosas_invitecode' operator='eq' value='{value.iosas_invitecode}' />
            //                            </filter>
            //                        </filter>
            //                        <order attribute='fullname' descending='false' />
            //                    </entity>
            //                </fetch>";
            //}

            var message = $"contacts?fetchXml=" + WebUtility.UrlEncode(fetchXml);
            var exists = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);

            int success = 100000000;
            int activity = 100000000;

            if (exists.IsSuccessStatusCode)
            {
                var root = JToken.Parse(exists.Content.ReadAsStringAsync().Result);

                if (root.Last().First().HasValues)
                {
                    string result = exists.Content.ReadAsStringAsync().Result;
                    dynamic json = JsonConvert.DeserializeObject(result);
                    string contactId = json.value[0].contactid.ToString();

                    if (json.value[0].iosas_externaluserid == null)
                    {
                        var updateValue = new JObject
                        {
                            { "iosas_externaluserid", value.iosas_externaluserid},
                            { "firstname", value.firstname},
                            { "lastname", value.lastname}
                        };

                        if (value.telephone1 != null)
                        {
                            updateValue["telephone1"] = value.telephone1;
                        }

                        var statement = $"contacts({contactId})";

                        HttpResponseMessage response = _d365webapiservice.SendUpdateRequestAsync(statement, updateValue.ToString());
                        if (!response.IsSuccessStatusCode)
                        {
                            return StatusCode((int)response.StatusCode, $"Failed to update contact record: {response.ReasonPhrase}");
                        }
                    }
                    //Log activity
                    Helper.LogUserActvity(success, activity, _d365webapiservice, contactId, value.firstname.ToString(), value.lastname.ToString());
                    string selectStatement = $"contacts({contactId})?$select=fullname,emailaddress1,contactid,firstname,lastname,telephone1,iosas_loginenabled,iosas_externaluserid,iosas_invitecode";
                    var respContactDetails = _d365webapiservice.SendRetrieveRequestAsync(selectStatement, true);
                    return Ok(respContactDetails.Content.ReadAsStringAsync().Result);
                }
                else //if (value.iosas_invitecode == null)  //If invite code is provided contact must already exist.  We don't create without invitation
                {
                    string selectStatement = "contacts?$select=fullname,emailaddress1,contactid,firstname,lastname,telephone1,iosas_loginenabled,iosas_externaluserid,iosas_invitecode";
                    var createResponse = _d365webapiservice.SendCreateRequestAsyncRtn(selectStatement, value.ToString());

                    if (createResponse.IsSuccessStatusCode)
                    {
                        //Log activity
                        string result = createResponse.Content.ReadAsStringAsync().Result;
                        dynamic json = JsonConvert.DeserializeObject(result);
                        string contactId = json.contactid.ToString();

                        Helper.LogUserActvity(success, activity, _d365webapiservice, contactId, value.firstname.ToString(), value.lastname.ToString());
                        return Ok(result);
                    }
                    else
                        return StatusCode((int)exists.StatusCode, $"Failed to create user: {exists.ReasonPhrase}");
                }
                //else
                //    return StatusCode((int)HttpStatusCode.Conflict, $"Failed to login with provided invite code.");
            }
            else
                return StatusCode((int)exists.StatusCode, $"Failed to retrieve user details: {exists.ReasonPhrase}");
        }

        // GET: api/contact
        [HttpGet("GetbyExternalId")]
        public ActionResult<string> Get(string externalId)
        {
            if (string.IsNullOrEmpty(externalId))
                return BadRequest("Invalid Request - BCeID or Invite Code is required.");

            //https://dev-ecc-iosas.apps.silver.devops.gov.bc.ca/school-application/app-1234?invitecode=guid

            var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='false' distinct='true'>
                                            <entity name='contact'>
                                                <attribute name='entityimage_url' />
                                                <attribute name='fullname' />
                                                <attribute name='emailaddress1' />
                                                <attribute name='contactid' />
                                                <attribute name='firstname' />
                                                <attribute name='lastname' />
                                                <attribute name='telephone1' />
                                                <attribute name='iosas_loginenabled' />
                                                <attribute name='iosas_externaluserid' />
                                                <attribute name='iosas_invitecode' />
                                                <filter type='and'>
                                                    <condition attribute='statecode' operator='eq' value='0' />
                                                    <filter type='or'>
                                                        <condition attribute='iosas_externaluserid' operator='eq' value='{externalId}' />
                                                        <condition attribute='emailaddress1' operator='eq' value='{externalId}' />
                                                    </filter>
                                                </filter>
                                                <order attribute='fullname' descending='false' />
                                            </entity>
                                        </fetch>";

            var message = $"contacts?fetchXml=" + WebUtility.UrlEncode(fetchXml);
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
                    return NotFound($"No Data: {externalId}");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }


        [HttpGet("GetById")]
        public ActionResult<string> GetById(string contactId)
        {
            if (string.IsNullOrEmpty(contactId))
                return BadRequest("Invalid Request - Contact Id is required.");

            //https://dev-ecc-iosas.apps.silver.devops.gov.bc.ca/school-application/app-1234?invitecode=guid
            string selectStatement = $"contacts({contactId})?$select=fullname,emailaddress1,contactid,firstname,lastname,telephone1,iosas_loginenabled,iosas_externaluserid,iosas_invitecode";

            var response = _d365webapiservice.SendRetrieveRequestAsync(selectStatement, true);
            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else
                return StatusCode((int)response.StatusCode, $"Failed to retrieve user details: {contactId}");

        }
    }
}