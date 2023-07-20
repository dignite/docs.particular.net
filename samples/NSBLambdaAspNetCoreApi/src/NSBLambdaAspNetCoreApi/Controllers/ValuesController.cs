using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Endpoint = NServiceBus.Endpoint;

namespace NSBLambdaAspNetCoreApi.Controllers;

[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    public IMessageSession MessageSession { get; }

    public ValuesController(IMessageSession messageSession)
    {
        MessageSession = messageSession;
    }

    // static ValuesController()
    //{
    //    var endpointConfig = new NServiceBus.EndpointConfiguration("AwsLambda.Sender");
    //    endpointConfig.SendFailedMessagesTo("ErrorAwsLambdaSQSTrigger");
    //    endpointConfig.UseSerialization<NewtonsoftJsonSerializer>();
    //    endpointConfig.UseTransport<SqsTransport>();
    //    serverlessEndpoint = Endpoint.Start(endpointConfig).Result;

    //}

    // GET api/values
    [HttpGet]
    public IEnumerable<string> Get()
    {
        throw new NotImplementedException("BOOM!!!");
        //return new string[] { "value1", "value2" };
    }

    // GET api/values/5
    [HttpGet("{id}")]
    public async Task<string> Get(int id)
    {
        //await serverlessEndpoint.Send("AwsLambdaSQSTrigger", new TriggerMessage() { Payload = id.ToString() }).ConfigureAwait(false);
        //return id.ToString();

        //Push a message to a queue. We could even uses the SQS client directly.
        //There is a new type of endpoint created by Sean, the one that makes it easy to push to sqs.
        //We can instantiate that directly from here.
        //The typical developer will prefer not to have to instantiate that every time and put it in a IoC Container.

        await MessageSession.Send("AwsLambdaSQSTrigger", new TriggerMessage() { Payload = id.ToString() })
            .ConfigureAwait(false);
            return id.ToString();
    }

    // POST api/values
    [HttpPost]
    public void Post([FromBody]string value)
    {
    }

    // PUT api/values/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody]string value)
    {
    }

    // DELETE api/values/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }

    //private static readonly IEndpointInstance? serverlessEndpoint;
}