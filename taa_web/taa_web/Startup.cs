using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(taa_web.Startup))]
namespace taa_web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
