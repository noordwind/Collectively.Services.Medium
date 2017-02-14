using System.IO;
using Medium.Services;
using Nancy;

namespace Coolector.Services.Medium.Modules
{
    public class WebhooksModule : ModuleBase
    {
        public WebhooksModule(IWebhookService webhookService) : base("webhooks", requireAuthentication: false)
        {
            Post("{endpoint}", async args => 
            {
                using(var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    await webhookService.ExecuteAsync(args.endpoint, Request.Query["trigger"], body, Request.Query["token"]);
                }

                return HttpStatusCode.OK;
            });
        }
    }
}