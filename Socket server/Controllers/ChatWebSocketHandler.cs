using Microsoft.Web.WebSockets;
using Socket_server.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace Socket_server.Controllers
{
    public class ChatWebSocketHandler : WebSocketHandler
    {
        private static WebSocketCollection _chatClients = new WebSocketCollection();
        private string _username;

        public ChatWebSocketHandler(string username)
        {
            _username = username;
        }

        public override void OnOpen()
        {
            _chatClients.Add(this);
        }

        public override void OnMessage(string message)
        {

            try
            {
                var zxc = HttpRuntime.AppDomainAppPath;
                var d = Directory.GetParent(zxc).Parent.FullName;

                string SuperString2 = JsonSerealization.ParseJsonToByteArray(message, d + "\\BinarySerialization/Manifest.json");

                byte[] arr = BinaryHelper.GetBytes(SuperString2);
                //"00000010 00000000000000000000000000000011 10 10 10 10 00000000000000000000000000000010 10 10 10 10 101000"
                arr = arr.Reverse().ToArray();

                _chatClients.Broadcast(arr);
            }
            catch(Exception exp) 
            {
                _chatClients.Broadcast(exp.ToString());

            }
        }
    }
}