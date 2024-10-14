using Apache.NMS.RestAPI.Interfaces;
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

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Add services to the container.

//Options

builder.Services.AddOptions<BusManagerSettings>()
    .BindConfiguration("BusManagerSettings")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<BusManagerSettings>>().Value);

//Background services

builder.Services.AddSingleton<MessageBusManagerService>();
builder.Services.AddSingleton<IHostedService, MessageBusManagerService>(
    serviceProvider => serviceProvider.GetService<MessageBusManagerService>());
builder.Services.AddSingleton<IBusManager, MessageBusManagerService>(
    serviceProvider => serviceProvider.GetService<MessageBusManagerService>());


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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NSM API V1");
    c.RoutePrefix = "";
});


app.UseHttpsRedirection();

app.UseCors(options => options.AllowAnyOrigin());

app.MapControllers();

app.UseMetricsAllMiddleware();
app.UseMetricsAllEndpoints();

app.Run();