using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Socket_server.Helpers
{
    internal static class HelperForJsonSerealization
    {
        public static Type GetTupeFromString(string type)
        {
            switch (type)
            {
                case "date":
                    return typeof(DateTime);
                case "datetime":
                    return typeof(DateTime);
                case "time":
                    return typeof(DateTime);
                case "bool":
                    return typeof(bool);
                case "int32":
                    return typeof(int);
                case "char":
                    return typeof(char);
                case "byte":
                    return typeof(sbyte);
                case "ubyte":
                    return typeof(byte);
                case "int16":
                    return typeof(short);
                case "uint16":
                    return typeof(ushort);
                case "uint32":
                    return typeof(uint);
                case "uint64":
                    return typeof(ulong);
                case "int64":
                    return typeof(long);
                case "float":
                    return typeof(float);
                case "string":
                    return typeof(string);
                case "double":
                    return typeof(double);
                case "currency":
                    return typeof(int);
                default: return null;
            }
        }

        public static string ToFoolByte(string OllBits)
        {
            string res = OllBits;
            int LengthToByt = res.Length % 8;

            if (LengthToByt != 0)
            {
                for (int i = 0; i < 8 - LengthToByt; i++)
                {
                    res += '0';
                }
            }

            return res;
        }

        private static List<JToken> MakeList(JObject dataForSerealizationJson, bool IS_Array)
        {
            List<JToken> res = new List<JToken>();
            IEnumerable<JToken> property;

            if (IS_Array)
            {
                property = ((JObject)dataForSerealizationJson.First.First).Property("data").Children().Children();
            }
            else
            {
                property = ((JObject)dataForSerealizationJson.First.First).Property("data").Children();
            }

            foreach (JToken item in property)
            {
                res.Add(item);
            }

            return res;
        }

        public static string GetBitsFromData(JObject dataForSerealizationJson, JToken manifestDataType, bool isArray)
        {
            string res = string.Empty;

            List<JToken> collection = MakeList(dataForSerealizationJson, isArray);

            if (isArray)
            {
                //res += '1';
                int cointCollection = collection.Count();

                byte[] Bytes = BinaryHelper.GetBytes(cointCollection, cointCollection.GetType());

                BitArray bits = new BitArray(Bytes);
                res += BinaryHelper.ToBitString(bits);
            }

            foreach (JObject item in collection)
            {
                res += getNameManifest(item, manifestDataType);
            }

            return res;
        }

        private static string getNameManifest(JObject item, JToken manifestDataType)
        {
            string res = string.Empty;

            IEnumerable<JProperty> property = ((JObject)manifestDataType).Properties();

            foreach (JProperty prop in property)
            {
                JToken bvn = item.GetValue(prop.Name);

                if (bvn != null)
                {
                    Type a = GetTupeFromString(prop.Value.ToString());
                    res += '1';

                    var ub = bvn.ToObject(a);

                    if (a == typeof(bool))
                    {
                        if ((bool)ub == true)
                        {
                            res += '1';
                        }
                        else
                        {
                            res += '0';
                        }
                    }
                    else if (a == typeof(DateTime))
                    {
                        res += BinaryHelper.DataBitsSerealization((DateTime)ub);
                    }
                    else
                    {
                        byte[] Bytes = BinaryHelper.GetBytes(ub, a);
                        BitArray bits = new BitArray(Bytes);
                        res += BinaryHelper.ToBitString(bits);
                    }
                }
                else
                {
                    res += '0';
                }
            }

            return res;

        }

        public static string getServerId(string json)
        {
            string res = string.Empty;

            dynamic jobject = JsonConvert.DeserializeObject(json);
            try
            {
                res = jobject.server.id.Value.ToString();
            }
            catch (Exception exp)
            {
                res = jobject.client.id.Value.ToString();
            }

            return res;
        }


        public static JObject getDataById(string id)
        {
            JObject res = new JObject();

            var zxc = HttpRuntime.AppDomainAppPath;
            var d = Directory.GetParent(zxc).Parent.FullName;

            string manifetFileJson = File.ReadAllText(d + "\\BinarySerialization/Manifest.json");

            JObject dataForSerealizationJson = JObject.Parse(manifetFileJson);

            IEnumerable<JToken> prop = dataForSerealizationJson.Property("server").Children().Children();

            foreach (JToken lol in prop)
            {
                if (((JObject)lol).Property("id").Value.ToString() == id)
                {
                    res = (JObject)lol;
                }
            }

            return res;
        }
    }
}