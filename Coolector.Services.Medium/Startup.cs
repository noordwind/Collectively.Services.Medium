using System;
using System.Collections.Generic;
using Coolector.Services.Medium.Framework;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lockbox.Client.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Medium;
using Medium.Integrations.AspNetCore;
using Medium.Providers.MyGet;
using Nancy.Owin;
using NLog.Extensions.Logging;
using NLog.Web;
using Medium.Integrations.Lockbox;

namespace Coolector.Services.Medium
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public IContainer ApplicationContainer { get; set; }
        public IServiceCollection Services { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .SetBasePath(env.ContentRootPath);

            if (env.IsProduction() || env.IsDevelopment())
            {
                builder.AddLockbox();
            }

            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMedium()
                    .AddMyGetProvider()
                    .AddInMemoryRepository();

            //TODO: Somehow Autofac + ASP.NET Core + Nancy doesn't work as expected.    
            Services = services;
            ApplicationContainer = GetServiceContainer(services);

            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();
            app.AddNLogWeb();
            env.ConfigureNLog("nlog.config");
            if (env.IsProduction() || env.IsDevelopment())
            {
                app.UseMedium(x => x.SettingsLoader = new LockboxMediumSettingsLoader(entryKey: "medium"));
            }
            else
            {
                app.UseMedium();
            }
            app.UseOwin().UseNancy(x => x.Bootstrapper = new Bootstrapper(Configuration, Services));
        }

        protected static IContainer GetServiceContainer(IEnumerable<ServiceDescriptor> services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);

            return builder.Build();
        }
    }
}