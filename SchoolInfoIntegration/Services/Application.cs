using ECC.Institute.CRM.IntegrationAPI.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
        private static List<JObject> _History = new();
        private static long HistoryLength = 100;
        public Application(ID365WebAPIService d365webapiservice, ILogger logger)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
            _logger = logger;
            _loopupService = new LookUpService(_d365webapiservice, logger);
        }

        public LookUpService LookUpSevice
        {
            get
            {
                return _loopupService;
            }
        }


        public static List<JObject> GetHistory()
        {
            return _History;
        }

        private static void SetHistory(D365ModelMetdaData meta, JObject result)
        {
            if (_History.Count > HistoryLength + 1)
            {
                _History.RemoveAt(0);
            }
            JObject entry = new()
            {
                ["time"] = DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss"),
                ["result"] = result
            };
            _History.Add(entry);
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
            return this.UpdateV2(authorities, AuthorityISFS.Create(authorities), new JObject()); //this.UpdateV2(authorities, SchoolAuthorityISFS.Create(authorities), new JObject());
        }
        public string SchoolUpsertIOSAS(School[] schools)
        {
            SchoolIOSAS meta = SchoolIOSAS.Create(schools);
            var lookupConfigs = new List<LookUpConfig>
            {
                new LookUpConfig(new SchoolAuthorityIOSAS(), School.SchoolAuthorityIds(schools)),
                new LookUpConfig(new SchoolDistrictIOSAS(), School.SchoolDistrictIds(schools))
            };
            var schoolLookUpForAuthorityAandDistrict = _loopupService.FetchLookUpByExternalId(lookupConfigs.ToArray());
            var schoolLookUps = SchoolLookupForIOSAS(meta);
            schoolLookUps.Merge(schoolLookUpForAuthorityAandDistrict);
            return this.UpdateV2(schools, meta, schoolLookUps);
        }
        public string SchoolUpsertForISFS(School[] schools)
        {
            SchoolISFS meta = SchoolISFS.Create(schools);
            var lookupConfigs = new List<LookUpConfig>
            {
                new LookUpConfig(new AuthorityISFS(), School.SchoolAuthorityIds(schools)),
                new LookUpConfig(new SchoolDistrictISFS(), School.SchoolDistrictIds(schools))
            };
            var schoolLookUpForAuthorityAandDistrict = _loopupService.FetchLookUpByExternalId(lookupConfigs.ToArray());
            var schoolLookUps = SchoolLookUpForISFS(meta);
            schoolLookUps.Merge(schoolLookUpForAuthorityAandDistrict);
            return this.UpdateV2(schools, meta, schoolLookUps);
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
        private JObject SchoolLookUpForISFS(SchoolISFS meta)
        {
            var relations = new List<D365ModelMetdaData>
            {
                new ISFSFundingGroup()
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
        private static JObject[]? CovertToValueArray(string json)
        {
            return JObject.Parse(json).GetValue("value")?.ToArray()?.Select(token => (JObject)token).ToArray();
        }
        public JObject[]? FilterByExternalId(D365ModelMetdaData meta, string value)
        {
            var query = meta.FilterAndSelectQueryOnExternalId(value);
            _logger.LogInformation($"FilterByExternalId: Will Filter Data using extern Query: {query}");
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, query);
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                _logger.LogInformation($"FilterByExternalId | {meta.tag} | Data by external id exists: {value} | {result}");
                return CovertToValueArray(result);
            }
            else
            {
                var excpMessage = $"FilterByExternalId | {meta.tag}({value}) | Fail (external id) | {ResponseDescription(response)}";
                _logger.LogInformation(excpMessage);
                return null;
            }

        }

        public JObject[]? FilterByBusiness(D365ModelMetdaData meta, string value)
        {

            // Data with external id not available go with default key
            var query = meta.FilterAndSelectQuery(value);
            _logger.LogInformation($"Filter | ${meta.tag} | Will Filter Data using Query(legacy): {query}");
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, query);
            if (response.IsSuccessStatusCode)
            {
                return CovertToValueArray(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                var excpMessage = $"Filter | {meta.tag}|{value}) | Fail | {ResponseDescription(response)}";
                _logger.LogInformation(excpMessage);
                throw new Exception(excpMessage);
            }

        }

        public JObject[]? Filter(D365ModelMetdaData meta, D365Model model)
        {
            // Try to get filter by external id
            JObject[]? reseultByExternalId = FilterByExternalId(meta, model.ExternalId());

            if (reseultByExternalId != null && reseultByExternalId.Length > 0)
            {
                return reseultByExternalId;
            }
            // Data with external id not available go with default key
            var query = meta.FilterAndSelectQuery(model.KeyValue());
            _logger.LogInformation($"Filter | ${meta.tag} | Will Filter Data using Query(legacy): {query}");
            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, query);
            if (response.IsSuccessStatusCode)
            {
                return CovertToValueArray(response.Content.ReadAsStringAsync().Result);
            }
            else
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
            result["metaClass"] = meta.GetType().Name;
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
            // result["lookups"] = lookUps;
            result["lookups-errors"] = lookUps["errors"];
            foreach (D365Model model in items)
            {
                // Get existing data

                JObject[]? existingObjects = null;
                string[] ids;
                JObject status = new JObject();
                status["key"] = model.KeyDisplay();
                status["type"] = meta.tag;
                try
                {
                    existingObjects = Filter(meta, model);
                }
                catch (Exception excp)
                {
                    status["filter_issue"] = excp.Message;
                }
                if (existingObjects != null && existingObjects.Length > 0)
                {
                    _logger.LogInformation($"UpdateV2 | {meta.tag}({model.KeyDisplay()}) | Existing value \n {existingObjects}");

                    status["existings"] = JArray.FromObject(existingObjects);

                    try
                    {
                        ids = existingObjects
                        .Where(existing =>
                        {
                            bool verifyResult = model.VerifyExisting(meta, (JObject)existing, _logger);
                            if (verifyResult == false)
                            {
                                _logger.LogInformation($"UpdateV2 | {meta.tag}({model.KeyDisplay()}) | Existing verfication fail: {existing}");
                                throw new Exception("Existing verification fail");

                            }
                            return verifyResult;
                        })
                        .Select(value => value[meta.primaryKey]?.ToString() ?? "")
                        .Where(value => value != null && value != "")
                        .ToArray();

                    }
                    catch
                    {
                        ids = new string[] { };
                        errors.Add("Existing verification fail");
                        status["existing_verify_status"] = "FAIL";
                        statuses.Add(status);
                        continue;
                    }

                }
                else
                {
                    _logger.LogInformation($"UpdateV2 |  {meta.tag}({model.KeyDisplay()}) | No existing value create");
                    ids = new string[] { };
                }

                try
                {
                    string resp;
                    if (ids.Length > 0)
                    {
                        _logger.LogInformation($"UpdateV2 |  {meta.tag}({model.KeyDisplay()}) | Will update {ids}");
                        status["has_duplicate"] = ids.Length > 1 ? true : false;
                        status["action"] = "update";
                        status["existing-ids"] = JArray.FromObject(ids);
                        resp = this.UpdateAtomic(model, meta, ids);
                    }
                    else
                    {
                        _logger.LogInformation($"UpdateV2 |  {meta.tag}({model.KeyDisplay()}) | Will create");
                        status["action"] = "create";
                        resp = this.CreateAtomic(model, meta);
                    }
                    status["status"] = "success";
                    status["info"] = JToken.Parse(resp);
                }
                catch (Exception excp)
                {
                    status["status"] = "error";
                    status["error"] = excp.Message;
                    errors.Add(excp.Message);
                }
                statuses.Add(status);
            }
            result["errors"] = JArray.FromObject(errors.ToArray());
            result["statuses"] = JArray.FromObject(statuses.ToArray());
            result["success"] = errors.Count > 0 ? false : true;
            SetHistory(meta, result);
            if (errors.Count > 0)
            {
                throw new Exception($"{result}");
            }
            return $"{result}";
        }

        public string UpdateAtomic(D365Model model, D365ModelMetdaData meta, string[] existings)
        {
            List<JObject> sucessStatuses = new();
            List<JObject> failureStatuses = new();
            var json = meta.GetD365DataModel(model);
            var body = json.ToString();
            JObject updateStatus = new();
            updateStatus["incoming"] = JToken.Parse(JsonConvert.SerializeObject(model, Formatting.None));
            updateStatus["outgoing"] = json;
            updateStatus["action"] = "update";
            _logger.LogInformation($"UpdateAtomic| D365 | {meta.tag} | model | {JToken.FromObject(model)}");
            _logger.LogInformation($"UpdateAtomic | D365 | {meta.tag} | Request {body}");
            foreach (string id in existings)
            {
                JObject opsStatus = new();
                var marker = $"UpdateAtomic | {meta.tag} | [{id}/ {model.KeyValue()}]";
                opsStatus["tag"] = marker;
                opsStatus["d365-id"] = id;
                var resp = _d365webapiservice.SendUpdateRequestAsync(meta.IdQuery(id), body);
                opsStatus["response"] = ResponseDescription(resp);
                if (resp.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"{marker} | Success | {ResponseDescription(resp)}");
                    opsStatus["status"] = "sucess";
                    sucessStatuses.Add(opsStatus);
                }
                else
                {
                    _logger.LogInformation($"{marker} | Fail | {ResponseDescription(resp)}");
                    opsStatus["status"] = "fail";
                    failureStatuses.Add(opsStatus);
                }
            }
            updateStatus["successStatuses"] = JToken.FromObject(sucessStatuses);
            updateStatus["failureStatuses"] = JToken.FromObject(failureStatuses);
            if (failureStatuses.Count > 1)
            {
                throw new Exception($"{JToken.FromObject(updateStatus)}");
            }
            return $"{JToken.FromObject(updateStatus)}";
        }
        private string CreateAtomic(D365Model model, D365ModelMetdaData meta)
        {
            JObject status = new();
            var json = meta.GetD365DataModel(model);
            var body = json.ToString();
            var marker = $"CreateAtomic | {meta.tag} | [{model.KeyValue()}] | body: {body}";
            var resp = _d365webapiservice.SendCreateRequestAsync($"{meta.entityName}", body);
            status["action"] = "Create";
            status["incoming"] = JToken.Parse(JsonConvert.SerializeObject(model, Formatting.None));
            status["outgoing"] = json;
            _logger.LogInformation($"CreateAtomic| D365 | {meta.tag} | model | {JToken.FromObject(model)}");
            _logger.LogInformation($"CreateAtomic | D365 | {meta.tag} | Request {body}");
            if (resp.IsSuccessStatusCode)
            {
                _logger.LogInformation($"{marker} | Success | {ResponseDescription(resp)}");
                status["status"] = "sucess";
                status["response"] = ResponseDescription(resp);
                return $"{status}";
            }
            else
            {
                _logger.LogInformation($"{marker} | Fail | {ResponseDescription(resp)}");
                status["status"] = "fail";
                status["body"] = json;
                status["response"] = ResponseDescription(resp);
                throw new Exception($"{status}");
            }
        }
    }

}
