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
        private readonly LookUpService _loopupService;
        public Application(ID365WebAPIService d365webapiservice, ILogger logger)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
            _logger = logger;
            _loopupService = new LookUpService(_d365webapiservice, logger);
        }

        public string DistrictUpsertIOSAS(SchoolDistrict[] districts)
        {
            SchoolDistrictIOSAS meta = SchoolDistrictIOSAS.Create(districts);
            return this.UpdateV2(districts, meta, DisctricLookUpForIOSAS());
        }

        public string DistrictUpsertISFS(SchoolDistrict[] districts)
        {
            SchoolDistrictISFS meta = SchoolDistrictISFS.Create(districts);
            return this.UpdateV2(districts, meta, DisctricLookUpForISFS());
        }

        public string AuthorityUpsertIOSAS(SchoolAuthority[] authorities)
        {
            return this.UpdateV2(authorities, SchoolAuthorityIOSAS.Create(authorities), new JObject());
        }

        public string AuthorityUpsertISFS(SchoolAuthority[] authorities)
        {
            return this.UpdateV2(authorities, SchoolAuthorityISFS.Create(authorities), new JObject());
        }

        public string SchoolUpsert(School[] schools)
        {
            SchoolIOSAS meta = SchoolIOSAS.Create(schools);
            return this.UpdateV2(schools, meta, SchoolLookupForIOSAS(meta));
        }
        public string GetData(D365Model model)
        {
            return _d365webapiservice.SendRetrieveRequestAsync(model.GetQuery(), true).Content.ReadAsStringAsync().Result;
        }

        private JObject SchoolLookupForIOSAS(SchoolIOSAS meta)
        {
            var relations = new List<D365ModelMetdaData>
            {
                new IOSASFundingGroup(),
                new IOSASInspcetionFundingGroup(),
                new IOSASOwnerOperator(),
                new IOSASTeam()
            };
            JObject result = _loopupService.FetchLookUpData(relations.ToArray());
            return result;
        }

        private JObject DisctricLookUpForIOSAS()
        {
            var relations = new List<D365ModelMetdaData>
            {
                new EduRegion()
            };
            JObject result = _loopupService.FetchLookUpData(relations.ToArray());
            return result;
        }

        private JObject DisctricLookUpForISFS()
        {
            var relations = new List<D365ModelMetdaData>
            {
                new EduRegionISFS()
            };
            JObject result = _loopupService.FetchLookUpData(relations.ToArray());
            return result;
        }

        private string[] Upsert(D365Model[] items, D365ModelMetdaData meta)
        {
            var resultsSuccess = new List<string>()
            {
                 $"Upsert | D365 | {meta.tag} | Following itmes are upserted"
            };
            var resultFailure = new List<string>
            {
                $"Upsert | D365 | {meta.tag} | Error: Received follwing errors"
            };
            foreach (D365Model model in items)
            {
                _logger.LogInformation($"Upsert: [{meta.tag}][Start]: {model.KeyDisplay()}");
                var resp = _d365webapiservice.UpsertRecord(model.KeyDisplay(), meta.GetD365DataModel(model).ToString());
                if (resp.IsSuccessStatusCode)
                {
                    var status = $"[{meta.tag}][Success] : {model.KeyDisplay()}";
                    _logger.LogInformation(status);
                    resultsSuccess.Add(status);
                }
                else
                {
                    var status = $"[{meta.tag}][Fail]: {model.KeyDisplay()} | {resp.StatusCode} | {resp.Content.ReadAsStringAsync().Result}";
                    _logger.LogInformation(status);
                    _logger.LogInformation($"[{meta.tag}][Fail]: URI: {resp.RequestMessage?.RequestUri}");
                    resultFailure.Add(status);
                }
            }
            if (resultFailure.Count > 1)
            {
                throw new Exception(string.Join("\n", resultFailure));
            }
            return resultsSuccess.ToArray();
        }
        private static string ResponseDescription(HttpResponseMessage message)
        {
            return D365ModelUtility.ResponseDescription(message);
        }
        private string? FilterByExternalId(D365ModelMetdaData meta, string value)
        {
            var query = meta.FilterAndSelectQueryOnExternalId(value);
            _logger.LogInformation($"Will Filter Data using extern Query: {query}");
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, query);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"FilterByExternalId | {meta.tag} | Data by external id exists: {value}");
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                var excpMessage = $"FilterByExternalId | {meta.tag}({value}) | Fail (external id) | {ResponseDescription(response)}";
                _logger.LogInformation(excpMessage);
                return null;
            }

        }
        private string Filter(D365ModelMetdaData meta, D365Model model)
        {
            // Try to get filter by external id
            var reseultByExternalId = FilterByExternalId(meta, model.ExternalId());
            if (reseultByExternalId != null)
            {
                return reseultByExternalId;
            }
            // Data with external id not available go with default key
            var query = meta.FilterAndSelectQuery(model.KeyValue());
            _logger.LogInformation($"Will Filter Data using Query: {query}");
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, query);
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            } else
            {
                var excpMessage = $"Filter | {meta.tag}({model.KeyValue()}) | Fail | {ResponseDescription(response)}";
                _logger.LogInformation(excpMessage);
                throw new Exception(excpMessage);
            }
            
        }
        private string UpdateV2(D365Model[] items, D365ModelMetdaData meta, JObject inputLookUps)
        {
            JObject result = new JObject();
            result["ops"] = "updateV2";
            result["tag"] = meta.tag;
            var errors = new List<string>();
            var failure = new List<string>();
            var statuses = new List<JObject>();
            // Case 1: No itmes
            if (items.Length < 1)
            {
                result["errors"] = JArray.FromObject(new string[] { "No item to update " });
                return result.ToString();
            }
            JObject lookUps = _loopupService.FetchLookUpValues(meta);
            lookUps.Merge(inputLookUps);
            result["lookups"] = lookUps;
            result["lookups-errors"] = lookUps["errors"];
            foreach (D365Model model in items)
            {
                // Get existing data
                
                JObject? existing = null;
                string[] ids;
                JObject status = new JObject();
                status["key"] = model.KeyDisplay();
                status["type"] = meta.tag;
                try
                {
                    var existingString = Filter(meta, model);
                    existing = JObject.Parse(existingString);
                } catch (Exception excp)
                {
                    status["filter_issue"] = excp.Message;
                }
                if (existing != null && existing?.GetValue("value")?.ToArray() is JToken[] values && values != null && values.Length > 0)
                {
                    _logger.LogInformation($"UpdateV2 | {meta.tag}({model.KeyDisplay()}) | Existing value \n {values}");
                    status["existings"] = JArray.FromObject(values);
                    ids = values
                        .Select(value => value[meta.primaryKey]?.ToString() ?? "")
                        .Where(value => value != null)
                        .ToArray();
                    
                } else
                {
                    _logger.LogInformation($"UpdateV2 |  {meta.tag}({model.KeyDisplay()}) | No existing value create");
                    ids = new string[] { };
                }

                try
                {
                    string resp;
                    if (ids.Length > 0)
                    {
                       
                        status["has_duplicate"] = ids.Length > 1 ? true : false;
                        status["action"] = "update";
                        resp = this.UpdateAtomic(model, meta, ids);
                    } else
                    {
                        status["action"] = "create";
                        resp = this.CreateAtomic(model, meta);
                    }
                    status["status"] = "success";
                } catch (Exception excp)
                {
                    status["status"] = "error";
                    status["error"] = excp.Message;
                    errors.Add(excp.Message);
                }
                statuses.Add(status);
            }
            result["errors"] = JArray.FromObject(errors.ToArray());
            result["statuses"] = JArray.FromObject(statuses.ToArray());
            if (errors.Count > 0)
            {
                throw new Exception($"{result}");
            }
            return $"{result}";
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
            var json = meta.GetD365DataModel(model);
            var body = json.ToString();
            _logger.LogInformation($"UpdateAtomic | D365 | {meta.tag} | Request {body}");
            foreach (string id in existings)
            {
                
                var resp = _d365webapiservice.SendUpdateRequestAsync(meta.IdQuery(id), body);
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
                    resultFailure.Add($"{marker} | Fail | Body | {json}");
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
            var json = meta.GetD365DataModel(model);
            var body = json.ToString();
            var marker = $"CreateAtomic | {meta.tag} | [{model.KeyValue()}] | body: {body}";
            var resp = _d365webapiservice.SendCreateRequestAsync($"{meta.entityName}", body);
            if (resp.IsSuccessStatusCode)
            {
                _logger.LogInformation($"{marker} | Success | {ResponseDescription(resp)}");
                return $"{marker} | Success | {ResponseDescription(resp)}";
            } else
            {
                _logger.LogInformation($"{marker} | Fail | {ResponseDescription(resp)}");
                throw new Exception($"{marker} | Fail | {ResponseDescription(resp)} | body | {json}");
            }
        }
    }

}
