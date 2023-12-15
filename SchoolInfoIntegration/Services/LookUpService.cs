using System;
using System.Linq;
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
		public JObject FetchLookUpData(D365ModelMetdaData[] items)
		{
			JObject result = new();
            JObject errors = new();
            foreach (D365ModelMetdaData meta in items)
			{
                try
                {
                    var query = meta.CustomSelectQuery();
                    _logger.LogInformation($"FetchLookUpData | {meta.tag} | Will Fetch data with query => {query}");
                    var resp = _d365webapiservice.SendMessageAsync(HttpMethod.Get, query);
                    if (resp.IsSuccessStatusCode)
                    {
                        try
                        {
                            var jsonResponse = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                            JObject[] values = (jsonResponse.GetValue("value")?.ToArray() ?? Array.Empty<JObject>())
                                .Select(item => (JObject)item).ToArray();

                            _logger.LogInformation($"FetchLookUpData | {meta.tag} | itmes: {items}");
                            result[meta.entityName] = JToken.FromObject(values);
                        }
                        catch (Exception excp)
                        {
                            errors[meta.entityName] = excp.Message;
                        }

                    }
                    else
                    {
                        var error = D365ModelUtility.ResponseDescription(resp);
                        _logger.LogInformation($"FetchLookUpData | {meta.tag} : Error: ${resp}");
                        errors[meta.entityName] = error;
                    }
                }
                catch (Exception ex)
                {
                    errors[meta.entityName] = $"FetchLookUpData: exception: {ex.Message}";
                }

            }
            result["errors"] = errors;
            return result;
		}
		public JObject FetchLookUpValues(D365ModelMetdaData meta)
		{
			JObject result = new();
			JObject errors = new();
			foreach(D365ModelMetdaData lookup in meta.lookupsMetaData)
			{
				string[]? values = meta.LookupValuesFor(lookup)?.ToList().Distinct().ToArray();
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
			meta.lookUps = result;
			return result;
		}

	}
}

