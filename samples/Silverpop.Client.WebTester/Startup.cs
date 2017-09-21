using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Nancy.Owin;

namespace Client.WebTester
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var rewriteOptions = new RewriteOptions()
                .AddRedirectToHttps();

            app.UseRewriter(rewriteOptions);

            app.UseStaticFiles();
            app.UseOwin(x => x.UseNancy());
        }
    }
}
