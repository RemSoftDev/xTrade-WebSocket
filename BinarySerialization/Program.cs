using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Socket_server.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Linq;

namespace BinarySerialization
{
    public class Program
    {
        public static void Main(string[] args)

        {
            var d = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

            //string SuperString2 = JsonSerealization.ParseJsonToByteArray(d + "/testVal.json", d + "/Client.json");

            // byte[] arr = BinaryHelper.GetBytes(SuperString2);

            //arr = arr.Reverse().ToArray();

            //Console.WriteLine(SuperString2);
            float a = 1.1f;
            
            byte[] Bytes = BinaryHelper.GetBytes(a, typeof(float));
            BitArray B_Id = new BitArray(Bytes);
            string res = BinaryHelper.ToBitString(B_Id);

            Console.WriteLine(res);

            Console.ReadLine();
        }
    }
}