using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Socket_server.Helpers
{
    public static class BinaryHelper
    {
        public static string DataBitsSerealization(DateTime date)
        {
            BitArray bitsYear = new BitArray(BitConverter.GetBytes((short)date.Year));

            BitArray bitsMonth = new BitArray(BitConverter.GetBytes((byte)date.Month));

            BitArray bitsDay = new BitArray(BitConverter.GetBytes((byte)date.Day));

            string getBitYear = ToBitString(bitsYear);
            getBitYear = getBitYear.Remove(1, 2);

            string getBitMonth = ToBitString(bitsMonth);
            getBitMonth = getBitMonth.Remove(0, 12);

            string getBitDay = BinaryHelper.ToBitString(bitsDay);
            getBitDay = getBitDay.Remove(0, 11);

            return getBitYear + getBitMonth + getBitDay;

        }

        public static byte[] GetBytes(object obj, Type objectType)
        {
            if (objectType == typeof(int)) return BitConverter.GetBytes((int)obj);
            else if (objectType == typeof(long)) return BitConverter.GetBytes((long)obj);
            else if (objectType == typeof(string))
            {
                List<byte> strBytes = new List<byte>();

                byte lastByte = new byte();

                foreach (byte Bytes in ASCIIEncoding.ASCII.GetBytes((string)obj))
                {
                    strBytes.Add(Bytes);

                }
                strBytes.Reverse();
                strBytes.Insert(0,lastByte);

                return strBytes.ToArray();

            }
            else if (objectType == typeof(bool)) return new byte[] { (bool)obj == true ? (byte)1 : (byte)0 };
            else if (objectType == typeof(double)) return BitConverter.GetBytes((double)obj);
            else if (objectType == typeof(byte)) return new byte[] { (byte)obj };
            else if (objectType == typeof(sbyte)) return new byte[] { (byte)((sbyte)obj) };
            else if (objectType == typeof(short)) return BitConverter.GetBytes((short)obj);
            else if (objectType == typeof(ushort)) return BitConverter.GetBytes((ushort)obj);
            else if (objectType == typeof(uint)) return BitConverter.GetBytes((uint)obj);
            else if (objectType == typeof(ulong)) return BitConverter.GetBytes((ulong)obj);
            else if (objectType == typeof(float)) return BitConverter.GetBytes((float)obj);
            else if (objectType == typeof(char))
            {
                byte[] ch = BitConverter.GetBytes((char)obj);
                byte[] ch1 = { ch[0]};
                return ch1;
            }
            else if (objectType == typeof(IntPtr))
            {
                throw new Exception("IntPtr type is not supported.");
            }
            else if (objectType == typeof(UIntPtr))
            {
                throw new Exception("UIntPtr type is not supported.");
            }
            else
            {
                throw new Exception("Could not retrieve bytes from the object type " + objectType.FullName + ".");
            }
        }

        public static object ReadBytes(byte[] bytes, Type objectType)
        {
            if (objectType == typeof(bool)) return bytes[0] == 1 ? true : false;
            else if (objectType == typeof(byte)) return bytes[0];
            else if (objectType == typeof(int)) return BitConverter.ToInt32(bytes, 0);
            else if (objectType == typeof(string))
            {
                string strBytes = string.Empty;

                for (int i = 0; i < bytes.Length; i += 2)
                {
                    strBytes += BitConverter.ToChar(bytes, i).ToString();
                }

                return strBytes;
            }
            else if (objectType == typeof(long)) return BitConverter.ToInt64(bytes, 0);
            else if (objectType == typeof(double)) return BitConverter.ToDouble(bytes, 0);
            else if (objectType == typeof(sbyte)) return (sbyte)bytes[0];
            else if (objectType == typeof(short)) return BitConverter.ToInt16(bytes, 0);
            else if (objectType == typeof(ushort)) return BitConverter.ToUInt16(bytes, 0);
            else if (objectType == typeof(uint)) return BitConverter.ToUInt32(bytes, 0);
            else if (objectType == typeof(ulong)) return BitConverter.ToUInt64(bytes, 0);
            else if (objectType == typeof(float)) return BitConverter.ToSingle(bytes, 0);
            else if (objectType == typeof(char)) return BitConverter.ToChar(bytes, 0);
            else if (objectType == typeof(IntPtr))
            {
                throw new Exception("IntPtr type is not supported.");
            }
            else
            {
                throw new Exception("Could not retrieve bytes from the object type " + objectType.FullName + ".");
            }
        }

        public static string ClassSerialization(object myInstance)
        {
            string ResultBitsSerealization = string.Empty;

            Type myType = myInstance.GetType();

            PropertyInfo[] properties = myInstance.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                Type type = property.PropertyType;

                if (property.PropertyType.Name.ToString().Contains("DateTime"))
                {
                    PropertyInfo myPropInfo = myType.GetProperty(property.Name.ToString());

                    string zs = DataBitsSerealization((DateTime)GetPropValue(myInstance, property.Name.ToString()));

                    ResultBitsSerealization += zs;

                }
                else if (!property.PropertyType.Name.Contains("List") && !property.PropertyType.Name.Contains("String"))
                {

                    PropertyInfo myPropInfo = myType.GetProperty(property.Name.ToString());

                    byte[] zs = GetBytes(GetPropValue(myInstance, property.Name.ToString()), GetPropValue(myInstance, property.Name.ToString()).GetType());

                    string daf = "";
                    foreach (byte f in zs)
                    {
                        daf += f.ToString() + " ";
                    }
                    BitArray bits = new BitArray(zs);

                    ResultBitsSerealization += ToBitString(bits);

                }

                else if (property.PropertyType.Name.ToString().Contains("List"))
                {
                    dynamic val = property.GetValue(myInstance);

                    Type piTheValue = property.PropertyType.GetGenericArguments()[0];
                    PropertyInfo[] myPropertyInfo = piTheValue.GetProperties();

                    byte[] countList = GetBytes(val.Count, val.Count.GetType());
                    BitArray bitsCountList = new BitArray(countList);

                    string byteCount = string.Empty;

                    foreach (byte bytes in countList)
                    {
                        byteCount += bytes.ToString();
                    }

                    foreach (var s in val)
                    {
                        foreach (PropertyInfo a in myPropertyInfo)
                        {

                            if (GetPropValue(s, a.Name) != null)
                            {
                                ResultBitsSerealization += '1';

                                if (a.PropertyType.Name.ToString().Contains("Boolean"))
                                {
                                    bool isTrue = (bool)GetPropValue(s, a.Name.ToString());
                                    if (isTrue)
                                    {
                                        ResultBitsSerealization += '1';
                                    }
                                    else
                                    {
                                        ResultBitsSerealization += '0';
                                    }

                                }
                                else
                                if (a.PropertyType.Name.ToString().Contains("DateTime"))
                                {
                                    PropertyInfo myPropInfo = myType.GetProperty(property.Name.ToString());

                                    string zs = DataBitsSerealization((DateTime)GetPropValue(s, a.Name.ToString()));

                                    ResultBitsSerealization += zs;
                                }
                                else
                                {
                                    byte[] zs = GetBytes(GetPropValue(s, a.Name), GetPropValue(s, a.Name).GetType());
                                    string daf = "";
                                    foreach (byte f in zs)
                                    {
                                        daf += f.ToString() + " ";
                                    }

                                    BitArray bits = new BitArray(zs);

                                    ResultBitsSerealization += ToBitString(bits);
                                }
                            }
                            else
                            {
                                ResultBitsSerealization += '0';
                            }
                        }
                    }
                }
            }

            int LengthToByt = ResultBitsSerealization.Length % 8;

            if (LengthToByt != 0)
            {
                for (int i = 0; i < 8 - LengthToByt; i++)
                {
                    ResultBitsSerealization += '0';
                }
            }

            return ResultBitsSerealization;
        }

        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static byte[] GetBytes(string bitString)
        {
            byte[] result = Enumerable.Range(0, bitString.Length / 8).
                Select(pos => Convert.ToByte(
                    bitString.Substring(pos * 8, 8),
                    2)
                ).ToArray();

            List<byte> mahByteArray = new List<byte>();
            for (int i = result.Length - 1; i >= 0; i--)
            {
                mahByteArray.Add(result[i]);
            }

            return mahByteArray.ToArray();
        }

        public static string ToBitString(BitArray bits)
        {
            StringBuilder sb = new StringBuilder();

            string c = string.Empty;

            for (int i = bits.Count - 1; i >= 0; i--)
            {

                if (bits[i])
                {
                    c += '1';
                }
                else
                {
                    c += '0';
                }

            }

            return c.ToString();
        }
    };
}
