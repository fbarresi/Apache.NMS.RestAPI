using Apache.NMS.RestAPI.Interfaces.Services;
using Apache.NMS.RestAPI.Interfaces.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Apache.NMS.RestAPI.Logic.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});


//Log

var loggingDirectory = builder.Configuration.GetValue(typeof(string), "LoggingDirectory") as string ?? "";

if (!Directory.Exists(loggingDirectory))
{
    Directory.CreateDirectory(loggingDirectory);
}

builder.Host
    .UseSerilog((ctx, lc) => 
        lc
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(Path.Combine(loggingDirectory,"Api-.log"), 
                rollingInterval: RollingInterval.Day, 
                rollOnFileSizeLimit:true,
                fileSizeLimitBytes:100*1024*1024,
                retainedFileTimeLimit: TimeSpan.FromDays(15),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    );

// Add services to the container.

//Options

builder.Services.AddOptions<MessageBusSettings>()
    .BindConfiguration("MessageBusSettings")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<MessageBusSettings>>().Value);

//Background services

builder.Services.AddSingleton<MessageBusService>();
builder.Services.AddSingleton<IHostedService, MessageBusService>(
    serviceProvider => serviceProvider.GetService<MessageBusService>());
builder.Services.AddSingleton<IBus, MessageBusService>(
    serviceProvider => serviceProvider.GetService<MessageBusService>());


// 

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers()
                .AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Apache.NMS.RestAPI",
        Description = "REST API to Apache.NMS",
        Contact = new OpenApiContact
        {
            Name = "Federico Barresi",
            Url = new Uri("https://github.com/fbarresi/Apache.NMS.RestAPI")
        }
    });
});

// Metrics

AppMetricsServiceCollectionExtensions.AddMetrics(builder.Services);
builder.Services.AddMetricsEndpoints();
builder.Services.AddMetricsTrackingMiddleware();
builder.Services.AddMvcCore().AddMetricsCore();

//

builder.Host.UseWindowsService();


//

var app = builder.Build();

app.MapGet("/health", () => "Ok!");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Space API V1");
    c.RoutePrefix = "";
});


app.UseHttpsRedirection();

app.UseCors(options => options.AllowAnyOrigin());

app.MapControllers();

app.UseMetricsAllMiddleware();
app.UseMetricsAllEndpoints();

app.Run();