using System;
using Newtonsoft.Json;
namespace ECC.Institute.CRM.IntegrationAPI.Model
{
	public class RegionMapper
	{
		internal Dictionary<string, string> RegionMap;
		static readonly RegionMapper _shared = new();
        public RegionMapper()
		{
            RegionMap = new Dictionary<string, string>()
			{
				{ "NOT_APPLIC", "0" },
				{ "KOOTENAYS", "1"},
				{ "OKANAGAN", "2"},
				{ "NORTHEAST", "3" },
				{ "FRASER", "4" },
				{ "METRO", "5" },
                { "VAN_ISLE", "6" },
                { "NORTHWEST", "7" },
                { "OFFSHORE", "8" },
                { "PSI", "9" },
                { "YUKON", "10" }
            };

        }
		public string? GetValueForRegion(string regionCode)
		{
			return RegionMap[regionCode];
		}
		public static RegionMapper Shared()
		{
			return _shared;
		}
	}
}

