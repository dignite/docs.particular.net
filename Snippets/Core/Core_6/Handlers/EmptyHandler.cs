﻿namespace Core6.Handlers
{
    using System.Threading.Tasks;
    using NServiceBus;

    #region EmptyHandler

    public class MyMessageHandler :
        IHandleMessages<MyMessage>
    {
        public async Task Handle(MyMessage message, IMessageHandlerContext context)
        {
            // do something in the client process
        }
    }

    #endregion
}
