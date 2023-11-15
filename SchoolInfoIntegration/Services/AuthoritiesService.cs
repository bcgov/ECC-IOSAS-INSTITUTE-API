using System;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ECC.Institute.CRM.IntegrationAPI
{
	public class AuthoritiesService : IAuthoritiesService
    {
		//private readonly ILogger _logger;
		public AuthoritiesService()
		{
            //_logger = logger;
            
        }

		public Boolean upsert(string applicationName, dynamic payload) {

            
            if (applicationName == "IOSAS")
			{
				return upsertForIOSAS(payload);
			}
			else if (applicationName == "ISFS")
			{
				return upsertForISFS(payload);
			}
			else {
                throw new NotImplementedException("Unkown application Name : " + applicationName);
            }
		}

		public Boolean upsertForIOSAS(dynamic value) {
            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger logger = factory.CreateLogger<Program>();

            logger.LogInformation("Prepping data for IOSAS");

            // Add Mapping
            //var conditions = string.IsNullOrEmpty(authorityName) ?
            //    "<filter type='and'><condition attribute='statecode' operator='eq' value='0'/></filter>" :
            //    $"<filter type='and'>" +
            //    $"<condition attribute='statecode' operator='eq' value='0'/>" +
            //    $"<condition attribute='edu_authority_type' operator='eq' value='757500000'/>" +
            //    $"<condition attribute='edu_name' operator='like' value='%{authorityName}%'/>" +
            //    $"</filter>";


            //var fetchXml = $@"<fetch version='1.0' mapping='logical' distinct='true'>
            //                    <entity name='edu_schoolauthority'>
            //                        <attribute name='edu_name'/>
            //                        <attribute name='edu_schoolauthorityid'/>
            //                        <attribute name='edu_vendorlocationcode'/>
            //                        <attribute name='edu_fax'/>
            //                        <attribute name='edu_email'/>
            //                        <attribute name='edu_address_street1'/>
            //                        <attribute name='edu_address_street2'/>
            //                        <attribute name='edu_address_city'/>
            //                        <attribute name='edu_address_province'/>
            //                        <attribute name='edu_address_country'/>
            //                        <attribute name='edu_address_postalcode'/>
            //                         {conditions}
            //                        <order attribute='edu_name' descending='false'/>
            //                    </entity>
            //                </fetch>";

            //var message = $"edu_schoolauthorities?fetchXml=" + WebUtility.UrlEncode(fetchXml);

            //var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            //if (response.IsSuccessStatusCode)
            //{
            //    var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

            //    if (root.Last().First().HasValues)
            //    {
            //        return Ok(response.Content.ReadAsStringAsync().Result);
            //    }
            //    else
            //    {
            //        return Ok($"[]");
            //    }
            //}
            //else
            //    return StatusCode((int)response.StatusCode,
            //        $"Failed to Retrieve records: {response.ReasonPhrase}");

            // Call D365 service

            // Handle errors
            return true;

		}

		public Boolean upsertForISFS(dynamic value) {
			return true;
		}
	}
}

