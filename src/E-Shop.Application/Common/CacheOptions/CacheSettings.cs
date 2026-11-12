using System;
using System.Collections.Generic;
using System.Text;

namespace E_Shop.Application.Common.CacheOptions
{
    public class CacheSettings
    {
        public static string SectionName { get; } = "CacheSettings";
        //number of minutes to cache the data
        public int SlidingExpirationMinutes { get; set; }
        //number of hour to cache the data
        public int AbsoluteExpirationHours { get; set; }
        public string RedisConnection { get; set; }
        public string Application { get; set; }
        public bool ByPassCache { get; set; }
    }
}
