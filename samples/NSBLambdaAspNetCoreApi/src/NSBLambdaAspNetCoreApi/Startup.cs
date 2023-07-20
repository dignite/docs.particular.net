using Microsoft.AspNetCore.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace NSBLambdaAspNetCoreApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                // using static System.Net.Mime.MediaTypeNames;
                context.Response.ContentType = Text.Plain;

                await context.Response.WriteAsync($"An exception was thrown. Exception:TN and CQ");

                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();

                if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
                {
                    await context.Response.WriteAsync(" The file was not found.");
                }

                if (exceptionHandlerPathFeature?.Path == "/")
                {
                    await context.Response.WriteAsync(" Page: Home.");
                }
            });
        });

        //if (env.IsDevelopment())
        //{
        app.UseDeveloperExceptionPage();
        //}

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });


        
    }
}