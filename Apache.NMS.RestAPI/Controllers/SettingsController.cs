using Apache.NMS.RestAPI.Interfaces.DTOs;
using Apache.NMS.RestAPI.Interfaces.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Apache.NMS.RestAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ILogger<SettingsController> logger;
    private readonly BusManagerSettings settings;

    public SettingsController(ILogger<SettingsController> logger, BusManagerSettings settings)
    {
        this.logger = logger;
        this.settings = settings;
    }

    [HttpGet]
    [Route("bus/names")]
    public Task<string[]> GetBusNames()
    {
        logger.LogInformation("requested bus names");
        return Task.FromResult(settings.BusSettings.Select(b => b.Name).ToArray());
    }
    
    [HttpGet]
    [Route("destination/names")]
    public Task<string[]> GetDestinationNames()
    {
        logger.LogInformation("requested destination names");
        return Task.FromResult(settings.Destinations.Keys.ToArray());
    }
}