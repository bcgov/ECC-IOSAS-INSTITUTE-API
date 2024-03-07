using System;
using ECC.Institute.CRM.IntegrationAPI.Model;
using Newtonsoft.Json.Linq;
using CsvHelper;
using CsvHelper.Configuration;



namespace ECC.Institute.CRM.IntegrationAPI
{
    public class ImporterFactory
    {
        public static ImpoterService Create(ID365WebAPIService d365WebAPIService, string application, ILogger logger)
        {
            var appSettings = d365WebAPIService.D365AuthenticationService.D365AppSettings.FirstOrDefault(a => a.Name.Equals(application, StringComparison.InvariantCultureIgnoreCase));
            if (appSettings == null)
            {
                throw new Exception($"ImpoterService: Application {application} is not supported.");
            }
            d365WebAPIService.Application = application;
            return new ImpoterService(d365WebAPIService, logger);
        }
    }
    public class ImportConfig
    {
        readonly string ConfigName;
        public readonly D365ModelMetdaData Meta;

        public ImportConfig(string name, D365ModelMetdaData meta)
        {
            this.ConfigName = name;
            this.Meta = meta;
        }
    }
    public class ImpoterService
    {
        private readonly ID365WebAPIService _d365webapiservice;
        private readonly ILogger _logger;
        private readonly Dictionary<string, ImportConfig> Config = new();
        private readonly Application Application;
        private readonly string iosas = "iosas";
        private readonly string isfs = "isfs";
        List<JObject> statuses = new();
        List<JObject> mainList = new();
        List<List<JObject>> splitList = new();
        List<JObject> emptyAtD365 = new();
        List<string> errors = new();
        DateTime currentJobStartTime = DateTime.Now;
        DateTime? currentJobEndTime;
        int currentIndex = -1;
        public Boolean isFinished = false;
        public Boolean isRunning = false;
        private string _StatusReport = "";
        static string ExternStatusKey = "d365_extern_key";

        private readonly Dictionary<string, Func<CsvReader, string, List<JObject>>> _importHandle = new();
        private void _AddConfig(D365ModelMetdaData meta, string app)
        {
            string name = $"{meta.entityName}-{app}";
            Config.Add(name, new(name, meta));
        }
        private void _AddHandler(D365ModelMetdaData meta, string app, Func<CsvReader, string, List<JObject>> action)
        {
            string name = $"{meta.entityName}-{app}";
            _importHandle.Add(name, action);
        }
        private List<JObject> HanldeSchoolAuthority(CsvReader csv, string app)
        {
            csv.Context.RegisterClassMap<AuthorityMapper>();
            return csv.GetRecords<SchoolAuthorityImport>().ToList().Select(item => app == iosas ? item.ToIOSAS() : item.ToISFS()).ToList();
        }
        private List<JObject> HandleSchooldDistrict(CsvReader csv, string app)
        {
            csv.Context.RegisterClassMap<SchoolDistrictMapper>();
            return csv.GetRecords<SchoolDistrictImport>().ToList().Select(item => app == iosas ? (JObject)item.ToIOSAS() : (JObject)item.ToISFS()).ToList();
        }
        private List<JObject> HandleSchool(CsvReader csv, string app)
        {
            csv.Context.RegisterClassMap<SchoolMapper>();
            return csv.GetRecords<SchoolImport>().ToList().Select(item => app == iosas ? item.ToIOSAS() : item.ToISFS()).ToList();
        }
        public ImpoterService(ID365WebAPIService d365webapiservice, ILogger logger)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
            _logger = logger;
            Application = new Application(d365webapiservice, logger);
            
            AuthorityISFS saISFS = new();
            _AddConfig(saISFS, isfs);
            _AddHandler(saISFS, isfs, (csv, app) => { return HanldeSchoolAuthority(csv, app); });
            SchoolAuthorityISFS saIOSAS = new();
            _AddConfig(saIOSAS, iosas);
            _AddHandler(saIOSAS, iosas, (csv, app) => { return HanldeSchoolAuthority(csv, app); });
            SchoolDistrictISFS sdISFS = new();
            _AddConfig(sdISFS, isfs);
            _AddHandler(sdISFS, isfs, (csv, app) => { return HandleSchooldDistrict(csv, app); });
            SchoolDistrictIOSAS sdIOSAS = new();
            _AddConfig(sdIOSAS, iosas);
            _AddHandler(sdIOSAS, iosas, (csv, app) => { return HandleSchooldDistrict(csv, app); });
            SchoolISFS schoolISFS = new();
            _AddConfig(schoolISFS, isfs);
            _AddHandler(schoolISFS, isfs, (csv, app) => { return HandleSchool(csv, app); });
            SchoolIOSAS schoolIOSAS = new();
            _AddConfig(schoolIOSAS, iosas);
            _AddHandler(schoolIOSAS, iosas, (csv, app) => { return HandleSchool(csv, app); });
        }

        public JObject TaskStatus()
        {
            return new()
            {
                new JProperty("current-index", currentIndex),
                new JProperty("startAt", currentJobStartTime),
                new JProperty("endAt", currentJobEndTime),
                new JProperty("isFinished", isFinished),
                new JProperty("isRunning", isRunning),
                new JProperty("emptyAtD365", JToken.FromObject(emptyAtD365)),
                new JProperty("statuses", JToken.FromObject(statuses)),
                new JProperty("errors", JToken.FromObject(errors))
            };
        }

        private void _Reset()
        {
            statuses = new();
            currentIndex = -1;
            currentJobEndTime = null;
            emptyAtD365 = new();
            errors = new();
            _StatusReport = ""; 
            _Clean();
        }
        private void _Clean()
        {
            splitList = new();
            mainList = new();
            isRunning = false;
            isFinished = true;
        }
        //private _handleImport
        public static List<List<T>> SplitListIntoChunks<T>(List<T> list, int chunkSize)
        {
            return list
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        private async void Start(D365ModelMetdaData meta, string key, Boolean verifyOnly = true)
        {
            splitList = SplitListIntoChunks(mainList, 40);
            isRunning = true;
            await Task.Run(() => {
                _logger.LogInformation($"ExternalIdImport: Will start");
                foreach (List<JObject> group in splitList)
                {
                    try
                    {
                        ProcessList(meta, key, group, verifyOnly);
                        _logger.LogInformation($"ExternalIdImport: Finish the group: {splitList.IndexOf(group)}");

                    } catch (Exception excp)
                    {
                        string message = ($"ExternalIdImport: Error processing group: {splitList.IndexOf(group)}: {excp.Message}");
                        _logger.LogError(message);
                        errors.Add(message);
                    }
                    Thread.Sleep(1000);
                    _StatusReport = StatusReport(meta);
                }
                // Clean
                currentJobEndTime = DateTime.Now;
                _StatusReport = StatusReport(meta);
                _Clean();
                _logger.LogInformation($"ExternalIdImport: [Finished]");
            });
        }
        private string StatusReport(D365ModelMetdaData meta)
        {
            StringWriter stringBuilder = new();
            stringBuilder.GetStringBuilder().AppendLine($"index,{meta.businessKey},{meta.primaryKey},{ExternStatusKey},d365,report,comment");
            foreach(JObject status in statuses)
            {
                string index = status["index"]?.ToString() ?? "NA";
                string bvalue = status[meta.businessKey]?.ToString() ?? "NA";
                string pValue = status[meta.primaryKey]?.ToString() ?? "NA";
                string eValue = status[ExternStatusKey]?.ToString() ?? "NA";
                string report = status["report"]?.ToString() ?? $"{true}";
                string d365 = status["d365"]?.ToString() ?? $"{false}";
                string comment = status["comment"]?.ToString() ?? "NA";
                stringBuilder.GetStringBuilder().AppendLine($"{index},{bvalue},{pValue},{eValue},{d365},{report},{comment}");
            }
            return stringBuilder.GetStringBuilder().ToString();
        }
        public string ImportReport()
        {
            return _StatusReport;
        }
        private string StartVerify(D365ModelMetdaData meta, string key)
        {
            splitList = SplitListIntoChunks(mainList, 40);
            isRunning = true;
            _logger.LogInformation($"ExternalIdImport: Will start");
            foreach (List<JObject> group in splitList)
            {
                try
                {
                    ProcessList(meta, key, group, true);
                    _logger.LogInformation($"ExternalIdImport: Finish the group: {splitList.IndexOf(group)}");

                }
                catch (Exception excp)
                {
                    string message = ($"ExternalIdImport: Error processing group: {splitList.IndexOf(group)}: {excp.Message}");
                    _logger.LogError(message);
                    errors.Add(message);
                }
                Thread.Sleep(2000);
            }
            // Clean
            currentJobEndTime = DateTime.Now;

            string result = $"{StatusReport(meta)}";
            _Clean();
            _logger.LogInformation($"ExternalIdImport: [Finished]");
            return result;

        }
        private static string ResponseDescription(HttpResponseMessage message)
        {
            return D365ModelUtility.ResponseDescription(message);
        }
        public string UpdateAtomic(JObject json, D365ModelMetdaData meta, string[] existings)
        {
            var resultsSuccess = new List<string>()
            {
                 $"UpdateAtomic | D365 | {meta.tag} | Following itmes are updated: {existings}"
            };
            var resultFailure = new List<string>
            {
                $"UpdateAtomic | D365 | {meta.tag} | Error: Received follwing errors: {existings}"
            };
            
            var body = json.ToString();
            _logger.LogInformation($"UpdateAtomic | D365 | {meta.tag} | Request {body}");
            foreach (string id in existings)
            {

                var resp = _d365webapiservice.SendUpdateRequestAsync(meta.IdQuery(id), body);
                var marker = $"UpdateAtomic | {meta.tag} | [{id}]";
                if (resp.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"{marker} | Success | {ResponseDescription(resp)}");
                    resultsSuccess.Add($"{marker} | Success | {ResponseDescription(resp)}");
                }
                else
                {
                    _logger.LogInformation($"{marker} | Fail | {ResponseDescription(resp)}");
                    resultFailure.Add($"{marker} | Fail | {ResponseDescription(resp)}");
                    resultFailure.Add($"{marker} | Fail | Body | {body}");

                }
            }
            if (resultFailure.Count > 1)
            {
                throw new Exception($"{JToken.FromObject(resultFailure)}");
            }
            return $"{JToken.FromObject(resultsSuccess)}";
        }
        private static Boolean _VerifyData(D365ModelMetdaData meta, JObject data)
        {
            return data[meta.businessKey]?.ToString() != null && data[meta.externalIdKey]?.ToString() != null;
        }
        private void ProcessList(D365ModelMetdaData meta, string key,  List<JObject> list, Boolean verifyOnly = true)
        {
            string[] values = list.Select(item => item.GetValue(meta.businessKey)?.ToString() ?? "").Where(value => value != "").ToArray();
            JObject[]? allD365 = Application.LookUpSevice.FetchLookupValues(meta, values);
            if (allD365 == null || allD365.Length == 0)
            {
                _logger.LogInformation($"ExternalIdImport: Not able to fetch any D365 object: {JToken.FromObject(list)}");
                errors.Add($"No D365 for {JToken.FromObject(values)}");
                emptyAtD365.AddRange(list);
                return;
            }
            foreach (JObject obj in list)
            {
                JObject status = JObject.FromObject(obj);
                currentIndex = mainList.IndexOf(obj);
                status["index"] = currentIndex;

                _logger.LogInformation($"ExternalIdImport: Index: {currentIndex}, Procesing object: {obj}");
                if (_VerifyData(meta, obj) && obj[meta.externalIdKey]?.ToString() is var externId &&
                    externId != null &&
                    obj[meta.businessKey]?.ToString() is var businessValue &&
                    businessValue != null)
                {
                    // Find key and value
                    
                    JObject[] selected = allD365.Where(d365 => d365[meta.businessKey]?.ToString() == businessValue).ToArray();
                    if (selected.Length > 0)
                    {
                        JObject[] haveExternalIds = selected.Where(d365 => d365[meta.externalIdKey]?.ToString() == externId).ToArray();
                        JObject[] noOrWrongExternalIds = selected.Where(d365 => d365[meta.externalIdKey]?.ToString() != externId || d365[meta.externalIdKey] == null).ToArray();
                        status["existings"] = JToken.FromObject(selected);
                        status["matchedObjects"] = JToken.FromObject(haveExternalIds);
                        status[ExternStatusKey] = selected[0].GetValue(meta.externalIdKey);
                        status[meta.primaryKey] = selected[0].GetValue(meta.primaryKey);
                        _logger.LogInformation($"ExternalIdImport: {businessValue}: Selected: {JToken.FromObject(selected)}");
                        if (noOrWrongExternalIds.Length > 0)
                        {
                            status["no-match"] = JToken.FromObject(noOrWrongExternalIds);
                            status["updated"] = false;
                            status["d365"] = false;
                            string[] existingIds = noOrWrongExternalIds.Select(item => item.GetValue(meta.primaryKey)?.ToString() ?? "").Where(str => str != "").ToArray();
                            status["d365-id"] = existingIds.Length > 0 ? existingIds[0] : null;
                            // Update here
                            if (verifyOnly == false && existingIds.Length > 0)
                            {
                                _logger.LogInformation($"ExternalIdImport: Index: {currentIndex} will update {businessValue}({externId}) => {existingIds}");
                                try
                                {
                                    UpdateAtomic(obj, meta, existingIds);
                                    _logger.LogInformation($"ExternalIdImport: Update sucess  {businessValue}({externId})");
                                    status["update-status"] = "sucess";
                                    status["report"] = false;
                                    status["updated"] = true;
                                    status["comment"] = "OK";
                                    Thread.Sleep(1000);
                                } catch (Exception exp)
                                {
                                    status["error"] = exp.Message;
                                    status["update-status"] = "fail";
                                    status["report"] = true;
                                    status["updated"] = false;
                                    status["comment"] = $"ExternalIdImport: No external id found. Error happens during update: {exp.Message}";
                                    _logger.LogError($"ExternalIdImport: Export fail {businessValue}({externId}): {exp.Message}");
                                }

                            } else
                            {
                                status["comment"] = $"No external id found for {businessValue}({externId})";
                            }
                        } else
                        {
                            
                            status["d365"] = true;
                            status["report"] = false;
                            status["comment"] = "OK";
                        }
                    } else
                    {
                        _logger.LogInformation($"ExternalIdImport: Unable to find D365 object from {meta.businessKey}={businessValue}[{externId}]");
                        status["comment"] = $"No in D365 item for value {meta.businessKey}={businessValue}({externId})";
                        status["existings"] = null;
                        status["report"] = true;
                        status["updated"] = false;
                        status["d365"] = false;
                    }
                }
                else
                {
                    status["comment"] = $"Unable to find external id {meta.externalIdKey} in record or buisness key: {meta.businessKey}";
                    _logger.LogInformation($"ExternalIdImport {key}: No external id in {obj} {meta.businessKey} {meta.externalIdKey}");
                }
                statuses.Add(status);
            }
        }
        public string ExternalIdImport(string applicationName, string entityName, CsvReader reader, Boolean verifyOnly = true)
        {
            string key = $"{entityName}-{applicationName}";
            try
            {
                if (Config[key] is ImportConfig config &&
                    config != null &&
                    _importHandle[key] is var handler &&
                    handler != null)
                {
                    _Reset();
                    currentJobStartTime = DateTime.Now;
                    List<JObject> list = handler(reader, applicationName);
                    mainList = list;
                    D365ModelMetdaData meta = config.Meta;
                    _logger.LogInformation($"ExternalIdImport: Will process data: {list.Count}");
                    Start(meta, key, verifyOnly);
                    return $"{TaskStatus()}";
                }
                throw new Exception($"Unable to find config for application:{applicationName}, entity:{entityName}");

            } catch (Exception ex)
            {
                _logger.LogError($"ExternalIdImport:Faile for {key} with error: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        public string Verify(string applicationName, string entityName, CsvReader reader)
        {
            string key = $"{entityName}-{applicationName}";
            try
            {
                if (Config[key] is ImportConfig config &&
                    config != null &&
                    _importHandle[key] is var handler &&
                    handler != null)
                {
                    _Reset();
                    currentJobStartTime = DateTime.Now;
                    List<JObject> list = handler(reader, applicationName);
                    mainList = list;
                    D365ModelMetdaData meta = config.Meta;
                    _logger.LogInformation($"ExternalIdImport: Will process data: {list.Count}");
                    return StartVerify(meta, key);
                }
                throw new Exception($"Unable to find config for application:{applicationName}, entity:{entityName}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"ExternalIdImport:Faile for {key} with error: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
    }
}


