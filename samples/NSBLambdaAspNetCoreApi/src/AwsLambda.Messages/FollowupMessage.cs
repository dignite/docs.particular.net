using NServiceBus;

public class FollowupMessage : IMessage
{
    public string Payload { get; set; }
}