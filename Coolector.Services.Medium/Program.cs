using Coolector.Common.Host;
using Coolector.Services.Medium.Framework;

namespace Coolector.Services.Medium
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebServiceHost
                .Create<Startup>(port: 11100)
                .UseAutofac(Bootstrapper.LifetimeScope)
                .UseRabbitMq(queueName: typeof(Program).Namespace)
                .Build()
                .Run();
        }
    }
}