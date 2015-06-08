using Owin;
using Owin.RequiresHttps;

namespace Silverpop.Client.WebTester
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
#if !DEBUG
            app.RequiresHttps(new RequiresHttpsOptions());
#endif

            app.UseNancy();
        }
    }
}