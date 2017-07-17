using Autofac;
using Microsoft.Extensions.Configuration;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using NLog;
using RawRabbit.Configuration;
using Collectively.Common.Extensions;
using Collectively.Common.Nancy;
using Collectively.Common.Exceptionless;
using Collectively.Common.RabbitMq;
using Collectively.Common.Security;
using Collectively.Common.Services;
using Nancy;
using Newtonsoft.Json;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Collectively.Services.Medium.Framework
{
    public class Bootstrapper : AutofacNancyBootstrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IExceptionHandler _exceptionHandler;
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        public static ILifetimeScope LifetimeScope { get; private set; }

        public Bootstrapper(IConfiguration configuration, IServiceCollection services)
        {
            _configuration = configuration;
            _services = services;
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope container)
        {
            base.ConfigureApplicationContainer(container);
            container.Update(builder =>
            {
                builder.Populate(_services);
                builder.RegisterType<CustomJsonSerializer>().As<JsonSerializer>().SingleInstance();
                builder.RegisterInstance(_configuration.GetSettings<ExceptionlessSettings>()).SingleInstance();
                builder.RegisterType<ExceptionlessExceptionHandler>().As<IExceptionHandler>().SingleInstance();
                //RabbitMqContainer.Register(builder, _configuration.GetSettings<RawRabbitConfiguration>());
                SecurityContainer.Register(builder, _configuration);
            });
            LifetimeScope = container;
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
        {
            pipelines.OnError.AddItemToEndOfPipeline((ctx, ex) =>
            {
                _exceptionHandler.Handle(ex, ctx.ToExceptionData(),
                    "Request details", "Collectively", "Service", "Medium");

                return ctx.Response;
            });
        }

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            pipelines.AfterRequest += (ctx) =>
            {
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization, Origin, X-Requested-With, Content-Type, Accept");
            };
            pipelines.SetupTokenAuthentication(container);
            _exceptionHandler = container.Resolve<IExceptionHandler>();
            Logger.Info("Collectively.Services.Medium API has started.");
        }
    }
}