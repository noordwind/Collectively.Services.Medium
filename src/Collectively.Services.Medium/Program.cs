using Collectively.Common.Host;
using Collectively.Services.Medium.Framework;

namespace Collectively.Services.Medium
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