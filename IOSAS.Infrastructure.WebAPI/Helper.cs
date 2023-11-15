using IOSAS.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xrm.Sdk.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace IOSAS.Infrastructure.WebAPI
{
    public class Helper
    {
        public static string LogUserActvity(int outcome, int activityType, ID365WebAPIService d365webapiservice, string? contactId, string? firstName=null, string? lastName = null)
        {
            string strOutcome = outcome == 100000000 ? "Success" : "Failure";
            string activityLabel = GetFieldPickListValue("iosas_portaluseractivity", "iosas_activitytype", activityType, d365webapiservice);

            //get contact detials
            string fullName=  string.Empty;
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                fullName = $"{firstName} {lastName}";
            }
            else if(!string.IsNullOrEmpty(contactId))
            {
                try
                {
                    var message = $"contacts({contactId})?$select=fullname";
                    var contactResponse = d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
                    string result = contactResponse.Content.ReadAsStringAsync().Result;
                    dynamic json = JsonConvert.DeserializeObject(result);
                    fullName = json.fullname.ToString();
                }
                catch (Exception ex)
                {
                    fullName = "Unknown user";
                }
            }
            else
            {
                fullName = "Unauthenticated user";
            }

            var log = new JObject
            {
                { "iosas_name",  $"{fullName} {activityLabel}: {strOutcome}" },
                { "iosas_outcome", outcome },
                { "iosas_activitytype", activityType }
            };

            if (!string.IsNullOrEmpty(contactId))
            {
                log["iosas_Contact@odata.bind"] = $"/contacts({contactId})";
            }

            string statement = $"iosas_portaluseractivities";
            var response = d365webapiservice.SendCreateRequestAsync(statement, log.ToString());

            if (response.IsSuccessStatusCode)
            {
                var entityUri = response.Headers.GetValues("OData-EntityId").First();

                string pattern = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
                Match m = Regex.Match(entityUri, pattern, RegexOptions.IgnoreCase);
                var newRecordId = string.Empty;
                if (m.Success)
                {
                    newRecordId = m.Value;
                    return newRecordId;
                }
                else
                    return "Failed to create log record";
            }
            else
                return "Failed to create log record";

        }

        public static string GetFieldPickListValue(string tableName, string fieldName, int value,  ID365WebAPIService d365webapiservice)
        {

            var message = $"stringmaps?$filter=objecttypecode eq '{tableName}' and attributename eq '{fieldName}' and attributevalue eq {value}";
          
            var response = d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().First().HasValues)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    dynamic json = JsonConvert.DeserializeObject(result);
                    return json.value[0].value;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
               return string.Empty;
        }
    }
}
