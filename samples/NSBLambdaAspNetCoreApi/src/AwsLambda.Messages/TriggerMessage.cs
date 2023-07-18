using NServiceBus;

public class TriggerMessage : IMessage
{
    public string Payload { get; set; }
}