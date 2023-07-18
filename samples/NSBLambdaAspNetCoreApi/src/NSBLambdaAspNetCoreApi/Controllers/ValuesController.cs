using Microsoft.AspNetCore.Mvc;
using NServiceBus;

namespace NSBLambdaAspNetCoreApi.Controllers;

[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    public IMessageSession MessageSession { get; }

    public ValuesController(IMessageSession messageSession) {
        MessageSession = messageSession;
    }

    // GET api/values
    [HttpGet]
    public IEnumerable<string> Get()
    {                
        return new string[] { "value1", "value2" };
    }

    // GET api/values/5
    [HttpGet("{id}")]
    public async Task<string> Get(int id)
    {
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
}