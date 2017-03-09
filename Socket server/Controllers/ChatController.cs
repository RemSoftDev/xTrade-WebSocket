using Microsoft.Web.WebSockets;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using System.Web.Http.Cors;


namespace Socket_server.Controllers
{

    [RoutePrefix("api/Chat")]
    public class ChatController : ApiController
    {

        public HttpResponseMessage Get(string username)
        {
            HttpContext.Current.AcceptWebSocketRequest(new ChatWebSocketHandler(username));

            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }


        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage getTradeContribs(string manifest)
        {
                ChatWebSocketHandler handler = new ChatWebSocketHandler("null");

            handler.OnMessage(manifest);

            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }

        public HttpResponseMessage Get()
        {
            var currentContext = HttpContext.Current;

            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {
                currentContext.AcceptWebSocketRequest(ProcessWebsocketSession);
            }

            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }

        private Task ProcessWebsocketSession(AspNetWebSocketContext context)
        {
            ChatWebSocketHandler handler = new ChatWebSocketHandler("a");
            var processTask = handler.ProcessWebSocketRequestAsync(context);

            return processTask;
        }
    }
}
