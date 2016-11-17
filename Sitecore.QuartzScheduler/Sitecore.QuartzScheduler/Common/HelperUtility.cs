using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.QuartzScheduler.Common
{
    public class HelperUtility
    {
        public static string GetJsonSerializedData(object data)
        {
            string jsonData = JsonConvert.SerializeObject(data, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            string requiredStructure = "{" +
                                    "\"data\": ";
            jsonData = jsonData.Insert(0, requiredStructure);
            jsonData = jsonData.Insert(jsonData.Length, "}");

            return jsonData;
        }
        public static string AddJsonHeader(string jsonData)
        {
            if (!String.IsNullOrEmpty(jsonData))
            {
                string requiredStructure = "{\"data\": {" +
                                            "\"dataset\": [";
                jsonData = jsonData.Insert(0, requiredStructure);
                jsonData = jsonData.Insert(jsonData.Length, "]}}");
            }
            return jsonData;
        }
    }
}