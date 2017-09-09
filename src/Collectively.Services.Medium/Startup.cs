using System;
using System.Collections.Generic;
using Collectively.Services.Medium.Framework;
using Collectively.Common.Logging;
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
using Nancy.Owin;
using Medium.Integrations.Lockbox;

namespace Collectively.Services.Medium
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public IContainer ApplicationContainer { get; set; }
        public IServiceCollection Services { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSerilog(Configuration);
            services.AddMedium()
                    .AddInMemoryRepository();

            //TODO: Somehow Autofac + ASP.NET Core + Nancy doesn't work as expected.    
            Services = services;
            ApplicationContainer = GetServiceContainer(services);

            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSerilog(loggerFactory);
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