using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Unobserver
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(config =>
            {
                config.AddConsole(options =>
                {
                    options.DisableColors = false;
                    options.IncludeScopes = true;
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        // The ONLY place to observe unobservable Task exceptions is this method:
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();

            Console.ForegroundColor = ConsoleColor.Yellow;

            try
            {
                if (e.Exception is AggregateException agex)
                {
                    agex.Handle(ex =>
                    {
                        Console.Error.WriteLine(ex.ToString());
                        Console.Error.WriteLine();
                        return true;
                    });
                }
                else
                {
                    Console.Error.WriteLine(e.Exception.ToString());
                    Console.Error.WriteLine();
                }
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
