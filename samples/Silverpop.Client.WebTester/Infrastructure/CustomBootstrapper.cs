using Microsoft.Extensions.Configuration;
using Nancy;
using Nancy.TinyIoc;

namespace Silverpop.Client.WebTester.Infrastructure
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        public CustomBootstrapper()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(RootPathProvider.GetRootPath())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.dev.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            var transactClientConfiguration = new TransactClientConfiguration();

            var configuration = Configuration.GetSection("silverpop");
            configuration.Bind(transactClientConfiguration);

            container.Register(new TransactClient(transactClientConfiguration));
        }
    }
}
