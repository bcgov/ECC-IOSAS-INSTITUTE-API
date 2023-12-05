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
    }
    
    public class D365ModelMetdaData
    {
        public string entityName;
        public string primaryKey;
        public string businessKey;
        public string tag;
        public List<D365ModelMetdaData> lookupsMetaData = new ();
        public JObject lookUps = new ();
        protected D365ModelMetdaData(string tag, string entity, string key, string businessKey)
        {
            this.tag = tag;
            entityName = entity;
            primaryKey = key;
            this.businessKey = businessKey;
        }
        public string SelectQuery()
        {
            return $"{entityName}?$select={businessKey},{primaryKey}";
        }
        public string FilterAndSelectQuery(string value)
        {
            return $"{entityName}?$select={businessKey},{primaryKey}&$filter={businessKey} eq '{value.Trim()}'";
        }
        public string FilterAndSelectLookUpQuery(string[] values)
        {
            var finalValues = values.Select(value => $"'{value}'");
            return $"{entityName}?$select={businessKey},{primaryKey}&$filter=Microsoft.Dynamics.CRM.In(PropertyName='{businessKey}',PropertyValues=[{string.Join(",",finalValues)}])";
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
    }



    class ModeledMetaData<T>: D365ModelMetdaData where T: D365Model
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
    }

    class SchoolDistrictIOSAS : ModeledMetaData<SchoolDistrict>
    {
        public SchoolDistrictIOSAS(): base("school-district", "edu_schooldistricts", "edu_schooldistrictid", "edu_internalcode")
        {
            // Adding lookups
            lookupsMetaData.Add(new EduRegion());

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
        public override JObject? TransformToD365(D365Application application, SchoolDistrict model)
        {

            return null;
        }
    }
}

