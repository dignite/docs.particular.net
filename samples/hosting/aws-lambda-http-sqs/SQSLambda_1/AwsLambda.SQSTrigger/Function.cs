using System;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

using NServiceBus;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SQSTrigger
{
    public class Function
    {
        public Function()
        {
        }

        #region SQSFunctionHandler
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            using var cancellationTokenSource = new CancellationTokenSource(context.RemainingTime.Subtract(DefaultRemainingTimeGracePeriod));

            await serverlessEndpoint.Process(evnt, context, cancellationTokenSource.Token);
        }
        #endregion

        static readonly TimeSpan DefaultRemainingTimeGracePeriod = TimeSpan.FromSeconds(10);

        #region HttpFunctionHandler
        [LambdaFunction()]
        [HttpApi(LambdaHttpMethod.Get, "/")]
        public async Task<string> Default(ILambdaContext context)
        {
            await serverlessEndpoint.Send(new TriggerMessage(), context);
            return $"{nameof(TriggerMessage)} sent.";
        }
        #endregion

        #region EndpointSetup
        private static readonly IAwsLambdaSQSEndpoint serverlessEndpoint = new AwsLambdaSQSEndpoint(context =>
        {
            var endpointConfiguration = new AwsLambdaSQSEndpointConfiguration("AwsLambdaSQSTrigger");
            endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();

            var routing = endpointConfiguration.RoutingSettings;

            routing.RouteToEndpoint(typeof(TriggerMessage), "AwsLambdaSQSTrigger");
            routing.RouteToEndpoint(typeof(BackToSenderMessage), "AwsLambda.Sender");

            var advanced = endpointConfiguration.AdvancedConfiguration;
            advanced.SendFailedMessagesTo("ErrorAwsLambdaSQSTrigger");

            return endpointConfiguration;
        });
        #endregion
    }
}