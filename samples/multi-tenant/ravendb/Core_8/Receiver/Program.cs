﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Operations.Expiration;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.MultiTenant.Receiver";

        var endpointConfiguration = new EndpointConfiguration("Samples.MultiTenant.Receiver");

        using (var documentStore = new DocumentStore
        {
            Urls = new[] { "http://localhost:8080" },
            Database = "MultiTenantSamples",
        })
        {
            documentStore.Initialize();
            await CreateDatabase(documentStore);
            await CreateTenantDatabase(documentStore, "A");
            await CreateTenantDatabase(documentStore, "B");

            var persistence = endpointConfiguration.UsePersistence<RavenDBPersistence>();
            persistence.SetDefaultDocumentStore(documentStore);

            endpointConfiguration.UseTransport(new LearningTransport());
            var outbox = endpointConfiguration.EnableOutbox();

            #region DetermineDatabase

            persistence.SetMessageToDatabaseMappingConvention(headers =>
            {
                return headers.TryGetValue("tenant_id", out var tenantId)
                    ? $"MultiTenantSamples-{tenantId}"
                    : "MultiTenantSamples";
            });

            #endregion

            #region DisableOutboxCleanup

            outbox.SetFrequencyToRunDeduplicationDataCleanup(Timeout.InfiniteTimeSpan);

            #endregion

            var pipeline = endpointConfiguration.Pipeline;

            pipeline.Register(new StoreTenantIdBehavior(), "Stores tenant ID in the session");
            pipeline.Register(new PropagateTenantIdBehavior(), "Propagates tenant ID to outgoing messages");

            var startableEndpoint = await Endpoint.Create(endpointConfiguration)
                .ConfigureAwait(false);


            var endpointInstance = await startableEndpoint.Start()
                .ConfigureAwait(false);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            if (endpointInstance != null)
            {
                await endpointInstance.Stop()
                    .ConfigureAwait(false);
            }
        }
    }

    static async Task CreateDatabase(DocumentStore documentStore)
    {
        var id = "MultiTenantSamples";
        try
        {
            await documentStore.Maintenance.ForDatabase(id).SendAsync(new GetStatisticsOperation());
        }
        catch (DatabaseDoesNotExistException)
        {
            try
            {
                await documentStore.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(id)));
            }
            catch (ConcurrencyException)
            {
            }
        }
    }

    static async Task CreateTenantDatabase(DocumentStore documentStore, string tenant)
    {
        #region CreateDatabase

        var id = $"MultiTenantSamples-{tenant}";
        try
        {
            await documentStore.Maintenance.ForDatabase(id).SendAsync(new GetStatisticsOperation());
        }
        catch (DatabaseDoesNotExistException)
        {
            try
            {
                await documentStore.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(id)));
            }
            catch (ConcurrencyException)
            {
            }
        }

        await documentStore.Maintenance.SendAsync(new ConfigureExpirationOperation(new ExpirationConfiguration
        {
            Disabled = false,
            DeleteFrequencyInSec = 60
        }));

        #endregion
    }
}