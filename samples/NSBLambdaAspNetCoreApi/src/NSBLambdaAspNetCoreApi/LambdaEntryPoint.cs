using Amazon.CloudWatchLogs.Model;
using Amazon.CloudWatchLogs;
using AWS.Logger.AspNetCore;
using NServiceBus;

namespace NSBLambdaAspNetCoreApi;

/// <summary>
/// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
/// actual Lambda function entry point. The Lambda handler field should be set to
/// 
/// NSBLambdaAspNetCoreApi::NSBLambdaAspNetCoreApi.LambdaEntryPoint::FunctionHandlerAsync
/// </summary>
public class LambdaEntryPoint :

    // The base class must be set to match the AWS service invoking the Lambda function. If not Amazon.Lambda.AspNetCoreServer
    // will fail to convert the incoming request correctly into a valid ASP.NET Core request.
    //
    // API Gateway REST API                         -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    // API Gateway HTTP API payload version 1.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    // API Gateway HTTP API payload version 2.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
    // Application Load Balancer                    -> Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction
    // 
    // Note: When using the AWS::Serverless::Function resource with an event type of "HttpApi" then payload version 2.0
    // will be the default and you must make Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction the base class.

    Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
{
    /// <summary>
    /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
    /// needs to be configured in this method using the UseStartup<>() method.
    /// </summary>
    /// <param name="builder"></param>
    protected override void Init(IWebHostBuilder builder)
    {
        builder
            .ConfigureServices(services =>
            {
                services.AddTransient<ILogger>(s => s.GetRequiredService<ILogger<Startup>>());
            })
            .ConfigureLogging(log =>
            {
                log.AddAWSProvider();
            })
            .UseStartup<Startup>();
    }

    /// <summary>
    /// Use this override to customize the services registered with the IHostBuilder. 
    /// 
    /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
    /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
    /// </summary>
    /// <param name="builder"></param>
    protected override void Init(IHostBuilder builder)
    {
        builder
            .UseNServiceBus(hostBuilderContext =>
            {
                var endpointConfiguration = new EndpointConfiguration("AwsLambda.Sender");
                endpointConfiguration.SendFailedMessagesTo("ErrorAwsLambdaSQSTrigger");
                endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
                endpointConfiguration.UseTransport<SqsTransport>();

                //await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
                return endpointConfiguration;
            })
            .ConfigureServices(services =>
            {
                services.AddTransient<ILogger>(s => s.GetRequiredService<ILogger<Startup>>());
            })
            .ConfigureLogging(log =>
            {
                log.AddAWSProvider();
            });
    }
}