using static System.Net.Mime.MediaTypeNames;
using ECC.Institute.CRM.IntegrationAPI.Model;
using Newtonsoft.Json.Linq;

namespace ECC.Institute.CRM.IntegrationAPI
{
    internal class ModelProcessUnit
    {
        public D365Model item;
        public string[] existing;
        internal ModelProcessUnit(D365Model item, string[] existing)
        {
            this.item = item;
            this.existing = existing;
        }
    }

    public class ApplicationFactory
    {
        public static Application Create(ID365WebAPIService d365WebAPIService, string name, ILogger logger)
        {
            var appSettings = d365WebAPIService.D365AuthenticationService.D365AppSettings.FirstOrDefault(a => a.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (appSettings == null)
            {
                throw new Exception($"Application {name} is not supported.");
            }
            d365WebAPIService.Application = name;
            System.Console.WriteLine($"Selected App Settings => {appSettings}");

            return new Application(d365WebAPIService, logger);
        }
    }

    /// <summary>
    /// Notes:  Investigate if Upsert canbe implemented using the apprach discussed in the link below.  Alternate keys must be set for the tables for this to work
    /// Otherwise check for existing records in CRM, then create or update and based on the outcome.
    /// https://learn.microsoft.com/en-us/power-apps/developer/data-platform/use-upsert-insert-update-record?tabs=webapi 
    /// </summary>
    public class Application
    {
        private readonly ID365WebAPIService _d365webapiservice;
        private readonly ILogger _logger;

        public Application(ID365WebAPIService d365webapiservice, ILogger logger)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
            _logger = logger;
        }

        public string[] DistrictUpsert(SchoolDistrict[] districts)
        {
            return this.Update(districts, D365ModelMetdaData.SchoolDistrict);
        }

        public string[] AuthorityUpsert(SchoolAuthority[] authorities)
        {
            return this.Upsert(authorities, "school-athority");
        }

        public string[] SchoolUpsert(School[] schools)
        {
            return this.Upsert(schools, "school");
        }
        public string GetData(D365Model model)
        {
            return _d365webapiservice.SendRetrieveRequestAsync(model.GetQuery(), true).Content.ReadAsStringAsync().Result;
        }

        private string[] Upsert(D365Model[] items, string tag)
        {
            var resultsSuccess = new List<string>()
            {
                 $"Upsert | D365 | {tag} | Following itmes are upserted"
            };
            var resultFailure = new List<string>
            {
                $"Upsert | D365 | {tag} | Error: Received follwing errors"
            };
            foreach (D365Model model in items)
            {
                _logger.LogInformation($"Upsert: [{tag}][Start]: {model.UpsertQuery()}");
                /*var existingStringResult = this.GetData(model);
               
                var result = JObject.Parse(existingStringResult);
                HttpResponseMessage resp;
                if (result != null)
                {
                    _logger.LogInformation($"Upsert: [{tag}]: existing:\n {existingStringResult}");
                    JToken[]? value = result.GetValue("value").ToArray();
                    resp = _d365webapiservice.SendCreateRequestAsync(model.IdQuery());
                } else
                {
                    _logger.LogInformation($"Upsert: [{tag}]: no existing:\n {existingStringResult}");
                }*/
                var resp = _d365webapiservice.UpsertRecord(model.UpsertQuery(), model.ToD365EntityModel().ToString());
                if (resp.IsSuccessStatusCode)
                {
                    var status = $"[{tag}][Success] : {model.UpsertQuery()}";
                    _logger.LogInformation(status);
                    resultsSuccess.Add(status);
                }
                else
                {
                    var status = $"[{tag}][Fail]: {model.UpsertQuery()} | {resp.StatusCode} | {resp.Content.ReadAsStringAsync().Result}";
                    _logger.LogInformation(status);
                    _logger.LogInformation($"[{tag}][Fail]: URI: {resp.RequestMessage?.RequestUri}");
                    resultFailure.Add(status);
                }
            }
            if (resultFailure.Count > 1)
            {
                throw new Exception(string.Join("\n", resultFailure));
            }
            return resultsSuccess.ToArray();
        }
        private string ResponseDescription(HttpResponseMessage message)
        {
            return $"Resp: URI: [{message.RequestMessage?.RequestUri}] | Status: {message.StatusCode}, ${message.ReasonPhrase} | Content: {message.Content.ReadAsStringAsync().Result}";
        }
        private string[] Update(D365Model[] items, D365ModelMetdaData meta)
        {
            var resultsSuccess = new List<string>()
            {
                 $"Update | D365 | {meta.tag} | Following itmes are upserted"
            };
            var resultFailure = new List<string>
            {
                $"Update | D365 | {meta.tag} | Error: Received follwing errors"
            };
            // Call All the records
            if (items.Count() < 1)
            {
                throw new Exception($"[Update][{meta.tag}] No item to update");
            }
            var allResp = _d365webapiservice.SendMessageAsync(HttpMethod.Get, meta.SelectQuery());
            if (allResp.IsSuccessStatusCode)
            {
                var allRecords = JObject.Parse(allResp.Content.ReadAsStringAsync().Result);
                if (allRecords != null)
                {
                    var proccessiongItems = new List<ModelProcessUnit>(); ;
                    JToken[]? existingValues = allRecords.GetValue("value")?.ToArray();
                    if (existingValues?.Length > 0)
                    {
                        foreach(D365Model model in items)
                        {
                            var matchings = existingValues
                                .Where(obj => obj[meta.businessKey]?.ToString() != null)
                                .Where((obj) => obj[meta.businessKey]?.ToString() == model.KeyValue())
                                .Select(obj => obj[meta.primaryKey]?.ToString() ?? "").ToArray();

                            if (matchings.Length > 0)
                            {
                                proccessiongItems.Add(new ModelProcessUnit(model, matchings));
                            } else
                            {
                                proccessiongItems.Add(new ModelProcessUnit(model, new string[] {}));
                            }
                        }
                    } else
                    {
                        // No previuos record, create new
                        foreach (D365Model model in items)
                        {
                            proccessiongItems.Add(new ModelProcessUnit(model, new string[] { }));
                        }
                    }
                    // Start processing Queue
                    foreach(ModelProcessUnit unit in proccessiongItems)
                    {
                        try
                        {
                            _logger.LogInformation($"[Update]{meta.tag} | Starting | model = {unit.item.KeyValue()}, {unit.existing}");
                            if (unit.existing.Length > 0)
                            {
                                resultsSuccess.Add(this.UpdateAtomic(unit.item, meta ,unit.existing));
                            }
                            else
                            {
                                resultsSuccess.Add(this.CreateAtomic(unit.item, meta)); 
                            }

                        } catch (Exception excp)
                        {
                            resultFailure.Add(excp.Message);
                        }
                    }
                }
                else
                {
                    throw new Exception($"[Update][{meta.tag}] Null records received: \n {ResponseDescription(allResp)}");
                }
            }
            else
            {
                throw new Exception($"[Update][{meta.tag}] Unable to find existing records: \n {ResponseDescription(allResp)}");
            }
            if (resultFailure.Count > 1)
            {
                throw new Exception(string.Join("\n", resultFailure));
            }
            return resultsSuccess.ToArray();
        }
        private string UpdateAtomic(D365Model model, D365ModelMetdaData meta, string[] existings)
        {
            var resultsSuccess = new List<string>()
            {
                 $"UpdateAtomic | D365 | {meta.tag} | Following itmes are updated: {existings}"
            };
            var resultFailure = new List<string>
            {
                $"UpdateAtomic | D365 | {meta.tag} | Error: Received follwing errors: {existings}"
            };
            
            foreach (string id in existings)
            {
                var resp = _d365webapiservice.SendUpdateRequestAsync(meta.IdQuery(id), model.ToD365EntityModel().ToString());
                var marker = $"UpdateAtomic | {meta.tag} | [{id}/ {model.KeyValue()}]";
                if (resp.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"{marker} | Success | {ResponseDescription(resp)}");
                    resultsSuccess.Add($"{marker} | Success | {ResponseDescription(resp)}");
                }
                else
                {
                    _logger.LogInformation($"{marker} | Fail | {ResponseDescription(resp)}");
                    resultFailure.Add($"{marker} | Fail | {ResponseDescription(resp)}");
                }
            }
            if (resultFailure.Count > 1)
            {
                throw new Exception(string.Join("\n", resultFailure));
            }
            return string.Join("\n", resultsSuccess);
        }
        private string CreateAtomic(D365Model model, D365ModelMetdaData meta)
        {
            var marker = $"Update | {meta.tag} | [{model.KeyValue()}]";
            var resp = _d365webapiservice.SendCreateRequestAsync($"{meta.entityName}", model.ToD365EntityModel().ToString());
            if (resp.IsSuccessStatusCode)
            {
                _logger.LogInformation($"{marker} | Success | {ResponseDescription(resp)}");
                return $"{marker} | Success | {ResponseDescription(resp)}";
            } else
            {
                _logger.LogInformation($"{marker} | Fail | {ResponseDescription(resp)}");
                throw new Exception($"{marker} | Fail | {ResponseDescription(resp)}");
            }
        }
    }

}
