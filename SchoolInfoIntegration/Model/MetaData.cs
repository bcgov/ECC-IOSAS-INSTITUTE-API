using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace ECC.Institute.CRM.IntegrationAPI.Model
{
    
    public class D365ModelMetdaData
    {
        public string entityName;
        public string primaryKey;
        public string businessKey;
        public string tag;
        public List<D365ModelMetdaData> lookupsMetaData = new ();
        public JObject lookUps = new ();
        public D365ModelMetdaData(string tag, string entity, string key, string businessKey)
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
    }
}

