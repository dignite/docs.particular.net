using NServiceBus;

public class BackToSenderMessage : IMessage
{
    public string Payload { get; set; }
}