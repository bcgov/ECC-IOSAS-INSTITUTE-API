using System;
using ECC.Institute.CRM.IntegrationAPI.Model;
using Newtonsoft.Json.Linq;

namespace ECC.Institute.CRM.IntegrationAPI
{
	public class LookUpService
	{
        private readonly ID365WebAPIService _d365webapiservice;
        private readonly ILogger _logger;
		
        public LookUpService(ID365WebAPIService service, ILogger logger)
		{
			_d365webapiservice = service;
			_logger = logger;
		}
		public JObject FetchLookUpValues(D365ModelMetdaData meta)
		{
			JObject result = new();
			JObject errors = new();
			foreach(D365ModelMetdaData lookup in meta.lookupsMetaData)
			{
				string[]? values = meta.LookupValuesFor(lookup);
				if (values?.Length > 0)
				{
					try
					{
                        var query = lookup.FilterAndSelectLookUpQuery(values);
                        _logger.LogInformation($"Lookup | {meta.tag} | ${lookup.tag} | Will Fetch data with query => {query}");
                        var resp = _d365webapiservice.SendMessageAsync(HttpMethod.Get, query);
                        if (resp.IsSuccessStatusCode)
                        {
                            try
                            {
								var jsonResponse = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
								var items = (jsonResponse.GetValue("value")?.ToArray() ?? new JToken[] {})
									.Select(item => (JObject)item).ToArray();

								_logger.LogInformation($"Lookup | {meta.tag} | ${lookup.tag} | itmes: {items}");
								result[lookup.entityName] = JToken.FromObject(items);
                            }
                            catch (Exception excp)
							{
								errors[lookup.entityName] = excp.Message;
							}

                    } else
                            {
                                var error = D365ModelUtility.ResponseDescription(resp);
                                _logger.LogInformation($"Lookup | {meta.tag} | ${lookup.tag}: Error: ${resp}");
                                errors[lookup.entityName] = error;
                            }
                        } catch (Exception ex)
					{
						errors[lookup.entityName] = ex.Message;
					}

                }
			}
			result["errors"] = errors;
			return result;
		}

	}
}

