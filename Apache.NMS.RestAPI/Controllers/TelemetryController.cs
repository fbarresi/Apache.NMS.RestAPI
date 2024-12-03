using Apache.NMS.RestAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Apache.NMS.RestAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TelemetryController : ControllerBase
{
    private readonly ILogger<TelemetryController> logger;
    private readonly IBusManager busManager;

    public TelemetryController(ILogger<TelemetryController> logger, IBusManager busManager)
    {
        this.logger = logger;
        this.busManager = busManager;
    }

    [HttpGet]
    [Route("state")]
    public Task<Dictionary<string, bool>> GetStates()
    {
        logger.LogInformation("requested bus state");
        return Task.FromResult(busManager.States);
    }
    
}