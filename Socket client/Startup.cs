using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Socket_client.Startup))]
namespace Socket_client
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
