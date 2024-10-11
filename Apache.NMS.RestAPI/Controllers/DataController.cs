using Apache.NMS.RestAPI.Interfaces;
using Apache.NMS.RestAPI.Interfaces.DTOs;
using Apache.NMS.RestAPI.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Apache.NMS.RestAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController : ControllerBase
{
    private readonly ILogger<DataController> logger;
    private readonly IBusManager busManager;

    public DataController(ILogger<DataController> logger, IBusManager busManager)
    {
        this.logger = logger;
        this.busManager = busManager;
    }

    [HttpPost]
    [Route("{bus}/send")]
    public Task Send([FromRoute]string bus, [FromBody]SendMessageDto messageDto)
    {
        logger.LogInformation("Received: {MessageDto}", messageDto.ToString());
        var messageBus = busManager.GetMessageBusByName(bus);
        var destination = busManager.GetDestinationByName(messageDto.Destination);
        return messageBus.Send(destination, messageDto.Payload);
    }
    
    [HttpPost]
    [Route("{bus}/request")]
    public Task Request([FromRoute]string bus, [FromBody]RequestMessageDto messageDto)
    {
        logger.LogInformation("Received: {MessageDto}", messageDto.ToString());
        var messageBus = busManager.GetMessageBusByName(bus);
        var destination = busManager.GetDestinationByName(messageDto.Destination);
        var replyDestination = messageDto.ReplyToType == ReplyTo.DestinationName ? busManager.GetDestinationByName(messageDto.ReplyTo) : destination;
        return messageBus.Request(destination, messageDto.Payload, messageDto.ReplyToType == ReplyTo.TemporaryQueue, replyDestination);
    }
    
    // [HttpGet]
    // public async IAsyncEnumerable<int> Get()
    // {
    //     for (int i = 0; i < 10; i++)
    //     {
    //         yield return i;
    //         await Task.Delay(1000);
    //     }
    // }
}