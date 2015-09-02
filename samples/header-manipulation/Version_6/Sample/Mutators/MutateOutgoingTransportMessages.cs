﻿using NServiceBus;
using NServiceBus.MessageMutator;
using NServiceBus.Unicast.Messages;

#region mutate-outgoing-transport-messages
public class MutateOutgoingTransportMessages : IMutateOutgoingTransportMessages
{
    IBus bus;

    public MutateOutgoingTransportMessages(IBus bus)
    {
        this.bus = bus;
    }

    public void MutateOutgoing(MutateOutgoingTransportMessagesContext context)
    {
        context.SetHeader("MutateOutgoingTransportMessages", "ValueMutateOutgoingTransportMessages");
    }
}
#endregion