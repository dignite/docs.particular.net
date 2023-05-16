using System;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using NServiceBus.TransactionalSession;

public class Program
{
    // for SqlExpress use Data Source=.\SqlExpress;Initial Catalog=nservicebus;Integrated Security=True;Encrypt=false
    public const string ConnectionString = @"Server=localhost,1433;Initial Catalog=nservicebus;User Id=SA;Password=yourStrong(!)Password;Encrypt=false;Connection Lifetime=80;";
    // public const string ConnectionString = @"Server=tcp:XYZ,3342;Persist Security Info=False;User ID=XYZ;Password=XYZ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

    public static void Main()
    {
        using var listener = new EventCounterListener();
        using (var myDataContext = new MyDataContext(new DbContextOptionsBuilder<MyDataContext>()
                   .UseSqlServer(new SqlConnection(ConnectionString))
                   .Options))
        {
            myDataContext.Database.EnsureCreated();
        }

        var host = Host.CreateDefaultBuilder()

            #region txsession-nsb-configuration
            .UseNServiceBus(context =>
            {
                var directory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, ".learningtransport");
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }

                var endpointConfiguration = new EndpointConfiguration("Samples.ASPNETCore.Sender");
                // var transportDefinition = new LearningTransport
                // {
                //     TransportTransactionMode = TransportTransactionMode.ReceiveOnly,
                //     StorageDirectory = directory
                // };
                var transport = endpointConfiguration.UseTransport<LearningTransport>();
                transport.Transactions(TransportTransactionMode.ReceiveOnly);
                transport.StorageDirectory(directory);
                endpointConfiguration.EnableInstallers();

                var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
                persistence.SqlDialect<SqlDialect.MsSqlServer>();
                persistence.ConnectionBuilder(() => new SqlConnection(ConnectionString));

                persistence.EnableTransactionalSession();

                endpointConfiguration.EnableOutbox();

                return endpointConfiguration;
            })
            #endregion

            #region txsession-ef-configuration
            .ConfigureServices(c =>
            {
                // Configure Entity Framework to attach to the synchronized storage session
                c.AddScoped(b =>
                {
                    var session = b.GetRequiredService<ISqlStorageSession>();
                    var context = new MyDataContext(new DbContextOptionsBuilder<MyDataContext>()
                        .UseSqlServer(session.Connection)
                        .Options);

                    //Use the same underlying ADO.NET transaction
                    context.Database.UseTransaction(session.Transaction);

                    //Ensure context is flushed before the transaction is committed
                    // session.OnSaveChanges((s, token) => context.SaveChangesAsync(token));
                    session.OnSaveChanges((s) => context.SaveChangesAsync());

                    return context;
                });
            })

            // .ConfigureServices(c =>
            // {
            //     c.AddDbContext<MyDataContext>(o =>
            //     {
            //         o.UseSqlServer(ConnectionString);
            //     });
            // })
            #endregion

            .ConfigureWebHostDefaults(c =>
            {
                c.ConfigureServices(s => s.AddControllers());
                c.Configure(app =>
                {
                    #region txsession-web-configuration
                    app.UseMiddleware<MessageSessionMiddleware>();
                    #endregion

                    // app.UseMiddleware<EfMiddleware>();
                    app.UseRouting();
                    app.UseEndpoints(r => r.MapControllers());
                });
                c.ConfigureLogging(l =>
                {
                    l.AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddFilter("NToastNotify", LogLevel.Warning)
                        .AddConsole();
                });
            })
            .Build();

        host.Run();
    }
}