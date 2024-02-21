using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace ECC.Institute.CRM.IntegrationAPI.Model
{

    public class JsonMapper
    {
        public delegate JToken? Map(JObject source, string sourceKey, string targetKey);
        public struct Mapper
        {
            public string TargetPath;
            public Map? Transformar;
        }
        public static JObject MapObject(JObject source, Dictionary<string, Mapper> mappers)
        {
            var target = new JObject();
            foreach(var kvo in mappers)
            {
                string targetKey = kvo.Key;
                Mapper mapper = kvo.Value;
                if (mapper.Transformar != null)
                {
                    try
                    {
                        target[targetKey] = mapper.Transformar(source, mapper.TargetPath, targetKey);
                    }
                    catch (Exception) { }
                } else
                {
                    
                }
            }
            return target;
        }
    }

    public class D365Application : IEquatable<D365Application>
    {
        public readonly string name;
        private D365Application(string name)
        {
            this.name = name;
        }
        public static D365Application FromString(string name)
        {
            if (name.ToLower() == "iosas")
            {
                return D365Application.IOSAS;
            } else if (name.ToLower() == "isfs")
            {
                return D365Application.ISFS;
            } else
            {
                throw new Exception($"D365Application: Unknown application name: {name}[{name.ToLower()}]");
            }
        }
        public override bool Equals(object? obj)
        {
            return this.Equals(obj as D365Application);
        }

        public bool Equals(D365Application? otherApp)
        {
            if (otherApp is null)
            {
                return false;
            }
            return this.name == otherApp?.name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
        public static bool operator ==(D365Application app1, D365Application app2)
        {
            return app1.Equals(app2);
        }
        public static bool operator !=(D365Application lhs, D365Application rhs) => !(lhs == rhs);

        // Public part
        public static D365Application IOSAS { get { return new D365Application("iosas"); } }
        public static D365Application ISFS { get { return new D365Application("isfs"); } }
        public static D365Application getApplication(string applicationName)
        {
            if (applicationName.ToLower() == "isfs")
            {
                return D365Application.ISFS;
            } else
            {
                return D365Application.IOSAS;
            }
        }
    }


    
    public class D365ModelMetdaData
    {
        public string entityName;
        public string primaryKey;
        public string businessKey;
        public string createDateColumn;
        public string externalIdKey;
        public string tag;
        public List<D365ModelMetdaData> lookupsMetaData = new ();
        public JObject lookUps = new ();
        protected D365ModelMetdaData(string tag, string entity, string key, string businessKey)
        {
            this.tag = tag;
            entityName = entity;
            primaryKey = key;
            this.businessKey = businessKey;
            createDateColumn = "createDateColumn";
            this.externalIdKey = "iosas_externalid";
        }
        public string SelectQuery()
        {
            return $"{entityName}?$select={businessKey},{primaryKey}";
        }
        public string[] OtherSelectedColumns()
        {
            return Array.Empty<string>();
        }
        protected string SelectStatement()
        {
            string[] otherColumns = OtherSelectedColumns();
            if (otherColumns.Length > 0)
            {
                string otherSelectedColumns = string.Join(",", otherColumns);
                return $"{businessKey},{primaryKey},{otherSelectedColumns}";
            }else
            {
                return $"{businessKey},{primaryKey}";
            }
        }
        public virtual string CustomSelectQuery()
        {
            return $"{entityName}?$select={SelectStatement()}";
        }
        public string FilterAndSelectQueryOnExternalId(string value)
        {
            return $"{entityName}?$select={SelectStatement()}&$filter={externalIdKey} eq '{value.Trim()}'";
        }
        public string FilterAndSelectQuery(string value)
        {
            return $"{entityName}?$select={SelectStatement()}&$filter={businessKey} eq '{value.Trim()}'";
        }
        public string FilterAndSelectLookUpQuery(string[] values)
        {
            var finalValues = values.Select(value => $"'{value}'");
            return $"{entityName}?$select={SelectStatement()},{primaryKey}&$filter=Microsoft.Dynamics.CRM.In(PropertyName='{businessKey}',PropertyValues=[{string.Join(",",finalValues)}])";
        }
        public string FilterAndSeclecQueryByExternalIds(string[] externalIds)
        {
            var finalValues = externalIds.Select(value => $"'{value}'");
            return $"{entityName}?$select={SelectStatement()},{primaryKey}&$filter=Microsoft.Dynamics.CRM.In(PropertyName='{externalIdKey}',PropertyValues=[{string.Join(",", finalValues)}])";
        }
        public string BuisnessKeyValue(JObject obj)
        {
            return obj.GetValue(businessKey)?.ToString() ?? "";
        }
        public string KeyValue(JObject obj)
        {
            return obj.GetValue(primaryKey)?.ToString() ?? "";
        }
        public string IdQuery(string id)
        {
            return $"{entityName}({id})";
        }
        public virtual string[]? LookupValuesFor(D365ModelMetdaData lookup) { return null; }
        public virtual JObject GetD365DataModel(D365Model model)
        {
            return new JObject();
        }
    }

    public class LookUpConfig
    {
        public D365ModelMetdaData Model;
        public string[] ExternalIds;
        public LookUpConfig(D365ModelMetdaData model, string[] ids)
        {
            this.Model = model;
            this.ExternalIds = ids;
        }
    }


    public class ModeledMetaData<T>: D365ModelMetdaData where T: D365Model
    {
        public T[]? values; 
        public ModeledMetaData(string tag, string entity, string key, string businessKey): base(tag, entity,  key,  businessKey)
        { 

        }
        public virtual JObject? TransformToD365(D365Application application, T model) { throw NotImplementedException(); }

        private Exception NotImplementedException()
        {
            throw new NotImplementedException();
        }

    }

    class EduRegion: D365ModelMetdaData
    {
        public EduRegion(): base("edu_region", "edu_regions", "edu_regionid", "edu_regionnumber") { }
        public override string CustomSelectQuery()
        {
            return $"{entityName}?$select=edu_regionid,edu_regionnumber,edu_name";
        }
    }
    class IOSASOwnerOperator: D365ModelMetdaData
    {
        public IOSASOwnerOperator() : base("iosas_owneroperator", "iosas_owneroperators", "iosas_owneroperatorid", "iosas_owneroperatornumber") { }

        override public string CustomSelectQuery()
        {
            return $"{entityName}?$select=iosas_name,iosas_owneroperatorid,iosas_owneroperatornumber&$filter=iosas_owneroperatornumber ne null";
        }

    }
    class IOSASFundingGroup: D365ModelMetdaData
    {
        public IOSASFundingGroup(): base("iosas_fundinggroup", "iosas_fundinggroups", "iosas_fundinggroupid", "iosas_name") { }
    }

    class IOSASInspcetionFundingGroup : D365ModelMetdaData
    {
        public IOSASInspcetionFundingGroup() : base("iosas_inspectionfundinggroup", "iosas_inspectionfundinggroups", "iosas_inspectionfundinggroupid", "iosas_name") { }
        public override string CustomSelectQuery()
        {
            return $"{entityName}?$select={SelectStatement()}&$filter=iosas_facilitycode ne null and iosas_schoolfundingcode ne null";
        }
    }
    class IOSASTeam: D365ModelMetdaData
    {
        public IOSASTeam() : base("team", "teams", "teamid", "name") { }

        override public string CustomSelectQuery()
        {
            return $"{entityName}?$select=teamid,teamtype,name,membershiptype,description&$filter=teamtype eq 0 and (name eq 'Independent Schools Branch' or name eq 'Offshore School Program')&$orderby=name";
        }
    }

    class IOSASSchoolGroup : D365ModelMetdaData
    {
        public IOSASSchoolGroup() : base("iosas_inspectionfundinggroup", "iosas_inspectionfundinggroup", "iosas_inspectionfundinggroups", "iosas_name") { }
    }


    class SchoolAuthorityIOSAS: ModeledMetaData<SchoolAuthority>
    {
        public SchoolAuthorityIOSAS() : base("edu_schoolauthority", "edu_schoolauthorities", "edu_schoolauthorityid", "edu_authority_no")
        {

        }

        public static SchoolAuthorityIOSAS Create(SchoolAuthority[] input)
        {
            SchoolAuthorityIOSAS obj = new()
            {
                values = input
            };

            return obj;
        }

        public override JObject GetD365DataModel(D365Model model)
        {
            return model.ToIOSAS(this.lookUps);
        }
    }

    class SchoolIOSAS: ModeledMetaData<School>
    {
        public SchoolIOSAS() : base("edu_school", "edu_schools", "edu_schoolid", "edu_mincode")
        {
            // Adding lookups
            // Authority
            lookupsMetaData.Add(new SchoolAuthorityIOSAS());
            lookupsMetaData.Add(new SchoolDistrictIOSAS());
            lookupsMetaData.Add(new IOSASOwnerOperator());
            lookupsMetaData.Add(new IOSASFundingGroup());
        }

        public static SchoolIOSAS Create(School[] input)
        {
            SchoolIOSAS obj = new()
            {
                values = input
            };

            return obj;
        }

        public override JObject GetD365DataModel(D365Model model)
        {
            return model.ToIOSAS(this.lookUps);
        }
    }

    class SchoolDistrictIOSAS : ModeledMetaData<SchoolDistrict>
    {
        public SchoolDistrictIOSAS(): base("edu_schooldistrict", "edu_schooldistricts", "edu_schooldistrictid", "edu_internalcode")
        {
        }
        public static SchoolDistrictIOSAS Create(SchoolDistrict[] input)
        {
            SchoolDistrictIOSAS obj = new()
            {
                values = input
            };
            return obj;
        }

        public override string[]? LookupValuesFor(D365ModelMetdaData lookup)
        {
            switch (lookup.entityName)
            {
                case "edu_regions":
                    return values?
                        .Where(value => value.DistrictRegionCode != "NOT_APPLIC")
                        .Select(item => item.DistrictRegionCode ?? "" )
                        .Where(item => String.IsNullOrEmpty(item) == false)
                        .ToArray();
                default:
                    return null;
            }
        }
        
        public override JObject GetD365DataModel(D365Model model)
        {
            return model.ToIOSAS(this.lookUps);
        }

    
    }
}

