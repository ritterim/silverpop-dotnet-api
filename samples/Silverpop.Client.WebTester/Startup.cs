using Owin;

namespace Silverpop.Client.WebTester
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy();
        }
    }
}