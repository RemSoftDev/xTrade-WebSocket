using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Socket_server.Helpers
{
    public class JsonSerealization
    {
        public static string AllStringBits;
        public static JToken MinifestById;

        public static string ParseJsonToByteArray(string dataForSerealization, string manifestFilwPath)
        {
            AllStringBits = string.Empty;
            string res = string.Empty;

            string dataForSerealizationFileJson = dataForSerealization;
            string manifetFileJson = File.ReadAllText(manifestFilwPath);

            //string a = HelperForJsonSerealization.getServerId(dataForSerealizationFileJson);

            JObject manifestByClientId = HelperForJsonSerealization.getDataById("17");

            var dataVal = manifestByClientId.ToString();
            var dataVal1 = manifestByClientId.Property("id");
            var dataValChild = manifestByClientId.ToString();


            var idObjeck = dataVal1.ToObject(typeof(byte));

            byte[] Bytes = BinaryHelper.GetBytes(idObjeck, typeof(byte));
            BitArray B_Id = new BitArray(Bytes);

            res += BinaryHelper.ToBitString(B_Id);


            string content = dataValChild;
            JToken token = JToken.Parse(content);
            JToken token2 = (JToken.Parse(dataForSerealizationFileJson));

            MinifestById = token;

            rec(token);
            List<JToken> adsdd = tryFind(token.Last);
            List<JToken> adsdd1 = tryFind(token2);
            //dynamic jobject = JsonConvert.DeserializeObject(dataForSerealizationFileJson);

            //bool isArray = IfDataArray();
            //JObject manifetJson = JObject.Parse(manifetFileJson);

            //JProperty manifetId = ((JObject)manifetJson.First.First).Property("id");
            //JProperty manifetName = ((JObject)manifetJson.First.First).Property("name");

            //JToken manifestDataType = manifetJson.First.First.Last.First;

            //JObject dataForSerealizationJson = JObject.Parse(dataForSerealizationFileJson);

            //JProperty dataForSerealizationId = ((JObject)dataForSerealizationJson.First.First).Property("id");
            //JProperty dataForSerealizationName = ((JObject)dataForSerealizationJson.First.First).Property("name");

            //JToken manifestDataValue = dataForSerealizationJson.First.First.Last.First;

            //Type aId = HelperForJsonSerealization.GetTupeFromString(manifetId.Value.ToString());

            //var idObjeck = dataForSerealizationId.ToObject(aId);

            //byte[] Bytes = BinaryHelper.GetBytes(idObjeck, aId);
            //BitArray B_Id = new BitArray(Bytes);

            //res += BinaryHelper.ToBitString(B_Id);
            //res += HelperForJsonSerealization.GetBitsFromData(dataForSerealizationJson, manifestDataType, isArray);
            //HelperForJsonSerealization.ToFoolByte(res);

            res += MakeBits(adsdd, adsdd1);
            res = HelperForJsonSerealization.ToFoolByte(res);

            return res;
        }

        public static string MakeBits(List<JToken> Maniflist1, List<JToken> Vallist2)
        {
            string res = string.Empty;

            for (int i = 0; i < Maniflist1.Count;)
            {
                JToken Manif = Maniflist1[0];

                bool ifFind = false;
                int numberArrRemove = 0;

                for (int j = 0; j < Vallist2.Count; j++)
                {
                    JToken Val = Vallist2[j];

                    var tampManifName = ((JProperty)Manif).Name;

                    var tampValName = ((JProperty)Val).Name;


                    if (tampManifName == tampValName)
                    {
                        ifFind = true;

                        if (tampValName.Contains("JArray"))
                        {
                            res += '1';

                            int massLength = (int)((JProperty)Val).Value;
                            byte[] Bytes = BinaryHelper.GetBytes(massLength, typeof(int));
                            BitArray B_Id = new BitArray(Bytes);
                            res += BinaryHelper.ToBitString(B_Id);

                            int indexAdd = (massLength * 2) - 1;


                            for (int masInd = j + 1; masInd < j + massLength + 1; masInd++)
                            {
                                if (((JProperty)Vallist2[1]).Name.Contains("Array"))
                                {
                                    for (int masElInd = (int)((JProperty)Vallist2[indexAdd]).Value; masElInd > 0; masElInd--)
                                    {
                                        if (Vallist2[indexAdd + 1].ToString() != "{}")
                                        {
                                            res += convertToStringBitsObject(((JProperty)Maniflist1[2]).Value, Vallist2[indexAdd + 1]);
                                        }
                                        Vallist2.RemoveAt(indexAdd + 1);
                                    }
                                    Vallist2.RemoveAt(indexAdd);
                                }
                                indexAdd -= 2;
                            }
                            Vallist2.RemoveAt(0);
                            Maniflist1.RemoveAt(2);
                            Maniflist1.RemoveAt(1);
                            Maniflist1.RemoveAt(0);
                        }
                        else if (((JProperty)Val).Name.ToString() == "isObjec")
                        {
                            res += '1';

                            Vallist2.RemoveAt(numberArrRemove);
                            Maniflist1.RemoveAt(numberArrRemove);
                            numberArrRemove++;

                            break;
                        }
                        else
                        {

                            JToken tampManifVal = ((JProperty)Manif).Value;
                            JToken tampValVal = ((JProperty)Val).Value;

                            res += convertToStringBitsObject(tampManifVal, Val);

                            Vallist2.RemoveAt(numberArrRemove);
                            Maniflist1.RemoveAt(numberArrRemove);
                            numberArrRemove++;

                        }

                        break;
                    }

                    else
                    {

                    }

                }
                if (!ifFind)
                {
                    if (((JProperty)Maniflist1[0]).Name == "isObjec")
                    {
                        Maniflist1.RemoveRange(0, (int)((JProperty)Maniflist1[0]).Value);
                    }
                    Maniflist1.RemoveAt(numberArrRemove);
                    numberArrRemove++;
                    res += '0';
                }
            }

            return res;
        }



        public static string convertToStringBitsObject(JToken tampManifVal, JToken Val)
        {
            string res = string.Empty;




            Type type = HelperForJsonSerealization.GetTupeFromString(tampManifVal.ToString());

            var HelperForJsonSerealizationInstens = Val.ToObject(type);

            if (tampManifVal.ToString() == "currency")
            {
                Type twype = typeof(double);
                int currency = (int)((double)Val.ToObject(twype) * 100);
                HelperForJsonSerealizationInstens = currency;
            }

            res += '1';

            if (type == typeof(bool))
            {
                if ((bool)HelperForJsonSerealizationInstens == true)
                {
                    res += '1';
                }
                else
                {
                    res += '0';
                }
            }
            else if (type == typeof(DateTime))
            {
                res += BinaryHelper.DataBitsSerealization((DateTime)HelperForJsonSerealizationInstens);
            }
            else
            {
                byte[] Bytes = BinaryHelper.GetBytes(HelperForJsonSerealizationInstens, type);
                BitArray bits = new BitArray(Bytes);
                res += BinaryHelper.ToBitString(bits);
            }

            return res;
        }


        public static bool IfDataArray(dynamic jobject)
        {
            bool res = false;

            if (jobject.server.data.Type == JTokenType.Array)
            {
                res = true;
            }

            return res;
        }

        public static string getManifestFile(string nameManifestFile)
        {
            string res = string.Empty;

            string[] filePaths = Directory.GetFiles(@"E:\CodeCraft\projects\xTrade WebSocket\BinarySerialization\",
                                                    nameManifestFile,
                                                    SearchOption.AllDirectories);

            return res;
        }


        public static string rec(JToken token)
        {
            try
            {
                foreach (var temp in token)
                {
                    JToken vc;

                    if (temp is JProperty)
                    {
                        vc = ((JProperty)temp).Value;
                    }
                    else
                    {
                        vc = (temp);
                    }
                    if (vc is JArray)
                    {
                        var Bytes = BinaryHelper.GetBytes((vc as JArray).Count, typeof(int));
                        BitArray B_Id = new BitArray(Bytes);
                        AllStringBits += BinaryHelper.ToBitString(B_Id);
                        rec(vc);
                    }
                    else if (vc is JObject)
                    {
                        if (!(vc as JObject).Path.Contains("data[") && (vc as JObject).Path != "data")
                        {
                            AllStringBits += '1';
                        }
                        rec(vc);
                    }
                    else
                    {

                        var a = getBinFromJtoken(temp);
                        AllStringBits += getBinFromJtoken(temp);
                    }
                }
            }
            catch (Exception exp)
            {

            }

            return "";
        }

        public static List<JToken> tryFind(JToken token)
        {
            List<JToken> OllistJson = new List<JToken>();

            getBinFromJtokenlolo(token, ref OllistJson);

            return OllistJson;
        }

        private static string getBinFromJtokenlolo(JToken token, ref List<JToken> OllistJson)
        {
            try
            {
                foreach (var temp in token)
                {
                    JToken vc;

                    if (temp is JProperty)
                    {
                        vc = ((JProperty)temp).Value;
                    }
                    else
                    {
                        vc = (temp);
                    }
                    if (vc is JArray)
                    {

                        string addName;
                        if ((JProperty)vc.First.First == null)
                        {
                            addName = "null";
                        }
                        else
                        {
                            addName = ((JProperty)vc.First.First).Name;
                        }
                        JProperty jt = new JProperty(name: "JArray" + addName, content: (vc as JArray).Count);

                        OllistJson.Add(jt);

                        var Bytes = BinaryHelper.GetBytes((vc as JArray).Count, typeof(int));
                        BitArray B_Id = new BitArray(Bytes);
                        AllStringBits += BinaryHelper.ToBitString(B_Id);
                        getBinFromJtokenlolo(vc, ref OllistJson);
                    }
                    else if (vc is JObject)
                    {

                        if ((vc as JObject).Path.Contains("valObject") && (vc as JObject).Path != "data")
                        {
                            JProperty jp = new JProperty(name: "isObjec", content: ((JObject)vc).Count);
                            OllistJson.Add(jp);
                        }
                        else if ((vc.Type.ToString() == "Object") && vc.ToString() == "{}")
                        {

                            JProperty jp = new JProperty(name: "NULLArray", content: 1);
                            OllistJson.Add(jp);
                            OllistJson.Add(vc);
                        }
                        else if ((vc.Type.ToString() == "Object") && (vc as JObject).Path != "server.data" && (vc as JObject).Path != "data")
                        {
                            JProperty jp = new JProperty(name: "Array" + ((JProperty)((JObject)vc).First).Name, content: ((JObject)vc).Count);
                            OllistJson.Add(jp);
                        }

                        getBinFromJtokenlolo(vc, ref OllistJson);
                    }
                    else
                    {
                        OllistJson.Add(temp);

                        var a = getBinFromJtoken(temp);
                        AllStringBits += getBinFromJtoken(temp);
                    }
                }
            }
            catch (Exception exp)
            {

            }

            return "";
        }


        private static string getBinFromJtoken(JToken token)
        {
            string res = string.Empty;


            Type a = HelperForJsonSerealization.GetTupeFromString(((JProperty)token).Value.ToString());

            MinifestById.Children();

            if (a != null)
            {
                string defTypeString = "lololo";
                var defType = GetDefault(a);
                char defChar = 'G';
                if (a == typeof(string))
                {
                    defType = defTypeString;
                }
                if (a == typeof(char))
                {
                    defType = defChar;
                }

                if (defType != null)
                {
                    res += '1';

                    if (a == typeof(bool))
                    {
                        if ((bool)defType == true)
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
                        res += BinaryHelper.DataBitsSerealization((DateTime)defType);
                    }
                    else
                    {
                        byte[] Bytes = BinaryHelper.GetBytes(defType, a);
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
        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}