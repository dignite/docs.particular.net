﻿using System;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.DynamoDB.Transactions.Client";
        var endpointConfiguration = new EndpointConfiguration("Samples.DynamoDB.Transactions.Client");
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.UseTransport<LearningTransport>();

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        Console.WriteLine("Press 'S' to send a StartOrder message to the server endpoint");

        Console.WriteLine("Press any other key to exit");

        while (true)
        {
            var key = Console.ReadKey();
            Console.WriteLine();

            var orderId = Guid.NewGuid();
            var startOrder = new StartOrder
            {
                OrderId = orderId
            };
            if (key.Key == ConsoleKey.S)
            {
                await endpointInstance.Send("Samples.DynamoDB.Transactions.Server", startOrder)
                    .ConfigureAwait(false);
                Console.WriteLine($"StartOrder Message sent to Server with OrderId {orderId}");
                continue;
            }
            break;
        }

        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}