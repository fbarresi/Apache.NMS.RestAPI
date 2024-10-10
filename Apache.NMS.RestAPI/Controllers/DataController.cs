using Apache.NMS.RestAPI.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Apache.NMS.RestAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController : ControllerBase
{
    private readonly ILogger<DataController> logger;
    private readonly IBus bus;

    public DataController(ILogger<DataController> logger, IBus bus)
    {
        this.logger = logger;
        this.bus = bus;
    }

    [HttpPost]
    public async Task<object> Send(string spaceDataDto)
    {
        logger.LogInformation($"Received Dto: {spaceDataDto}");
        var reply = await bus.SendAsync(spaceDataDto);
        logger.LogInformation($"Sending Dto: {reply}");
        return reply;
    }
}