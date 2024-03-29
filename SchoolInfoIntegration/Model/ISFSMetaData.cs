﻿using System;
using Newtonsoft.Json.Linq;
namespace ECC.Institute.CRM.IntegrationAPI.Model
{
	public class ISFSMetaData: D365ModelMetdaData
    {
        protected ISFSMetaData(string tag, string entity, string key, string businessKey): base(tag, entity, key, businessKey)
        {
            this.externalIdKey = "edu_externalid";
        }
    }

    public class ISFSSModeledMetaData<T> : ISFSMetaData where T : D365Model
    {
        public T[]? values;
        public ISFSSModeledMetaData(string tag, string entity, string key, string businessKey) : base(tag, entity, key, businessKey)
        {

        }
        public virtual JObject? TransformToD365(D365Application application, T model) { throw NotImplementedException(); }

        private Exception NotImplementedException()
        {
            throw new NotImplementedException();
        }

    }

    class EduRegionISFS : ISFSMetaData
    {
        public EduRegionISFS() : base("edu_region", "edu_regions", "edu_regionid", "edu_internalcode") { }
        public override string CustomSelectQuery()
        {
            return $"{entityName}?$select=edu_regionid,edu_internalcode,edu_name";
        }
    }

    class ISFSFundingGroup : D365ModelMetdaData
    {
        public ISFSFundingGroup() : base("isfs_fundinggroup", "isfs_fundinggroups", "isfs_fundinggroupid", "isfs_name") { }
    }

    // edu_school
    // edu_schoolauthority
    // edu_schooldistrict
    public class SchoolAuthorityISFS: ISFSSModeledMetaData<SchoolAuthority>
	{
        // TableName: edu_schoolauthority
        public SchoolAuthorityISFS() : base("edu_schoolauthority", "edu_schoolauthorities", "edu_schoolauthorityid", "edu_authority_no")
        {

        }

        public static SchoolAuthorityISFS Create(SchoolAuthority[] input)
        {
            SchoolAuthorityISFS obj = new()
            {
                values = input
            };

            return obj;
        }

        public override JObject GetD365DataModel(D365Model model)
        {
            return model.ToISFS(this.LookUps);
        }
    }

    public class AuthorityISFS : ISFSSModeledMetaData<SchoolAuthority>
    {
        // TableName: edu_schoolauthority
        public AuthorityISFS() : base("isfs_authority", "isfs_authorities", "isfs_authorityid", "isfs_authorityno")
        {

        }

        public static AuthorityISFS Create(SchoolAuthority[] input)
        {
            AuthorityISFS obj = new()
            {
                values = input
            };

            return obj;
        }

        public override JObject GetD365DataModel(D365Model model)
        {
            return (model is SchoolAuthority authority) ? authority.ISFS(this.LookUps) : model.ToIOSAS(this.LookUps);
        }
    }



    public class SchoolDistrictISFS: ISFSSModeledMetaData<SchoolDistrict>
    {
        public SchoolDistrictISFS() : base("edu_schooldistrict", "edu_schooldistricts", "edu_schooldistrictid", "edu_internalcode")
        {
            // Adding lookups
            lookupsMetaData.Add(new EduRegionISFS());

        }
        public static SchoolDistrictISFS Create(SchoolDistrict[] input)
        {
            SchoolDistrictISFS obj = new()
            {
                values = input
            };
            return obj;
        }

        public override string[]? LookupValuesFor(D365ModelMetdaData lookup)
        {
            /*switch (lookup.entityName)
            {
                case "edu_regions":
                    return values?
                        .Where(value => value.DistrictRegionCode != "NOT_APPLIC")
                        .Select(item => item.DistrictRegionInternalCode ?? "")
                        .Where(item => String.IsNullOrEmpty(item) == false)
                        .ToArray();
                default:
                    return null;
            }*/
            return null;
        }

        public override JObject GetD365DataModel(D365Model model)
        {
            return model.ToISFS(this.LookUps);
        }
    }

    class SchoolISFS : ISFSSModeledMetaData<School>
    {
        public SchoolISFS() : base("edu_school", "edu_schools", "edu_schoolid", "edu_mincode")
        {
            // Adding lookups
            // Authority
            
        }

        public static SchoolISFS Create(School[] input)
        {
            SchoolISFS obj = new()
            {
                values = input
            };

            return obj;
        }

        public override JObject GetD365DataModel(D365Model model)
        {
            return model.ToISFS(this.LookUps);
        }
    }
}

