using Autofac;
using Nancy.Bootstrappers.Autofac;

namespace Silverpop.Client.WebTester.Infrastructure
{
    public class CustomBootstrapper : AutofacNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer)
        {
            var builder = new ContainerBuilder();
            builder
                .Register<TransactClient>(x => TransactClient.CreateUsingConfiguration())
                .SingleInstance();

            builder.Update(existingContainer.ComponentRegistry);

            base.ConfigureApplicationContainer(existingContainer);
        }
    }
}