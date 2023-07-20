using Microsoft.AspNetCore.Server.Kestrel;
using System.Net;
using NServiceBus;
using Endpoint = NServiceBus.Endpoint;

namespace NSBLambdaAspNetCoreApi;


/// <summary>
/// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
/// </summary>
public class LocalEntryPoint
{
    public static void Main(string[] args)
    {
        //var endpointConfiguration = new EndpointConfiguration("AwsLambda.Sender");
        //endpointConfiguration.SendFailedMessagesTo("ErrorAwsLambdaSQSTrigger");
        //endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
        //endpointConfiguration.UseTransport<SqsTransport>();

        //sqsEndpoint = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            //.UseNServiceBus(hostBuilderContext  =>
            //{
            //    var endpointConfiguration = new NServiceBus.EndpointConfiguration("AwsLambda.Sender");
            //    endpointConfiguration.SendFailedMessagesTo("ErrorAwsLambdaSQSTrigger");
            //    endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
            //    endpointConfiguration.UseTransport<SqsTransport>();

            //    //await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            //    return endpointConfiguration;
            //})
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}