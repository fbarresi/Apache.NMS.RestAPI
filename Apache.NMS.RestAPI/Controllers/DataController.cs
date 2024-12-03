using Apache.NMS.RestAPI.Interfaces;
using Apache.NMS.RestAPI.Interfaces.DTOs;
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
        var destination = busManager.GetDestinationByName(messageDto.Destination) ?? messageDto.Destination;
        return messageBus.Send(destination, messageDto.Payload);
    }
    
    [HttpPost]
    [Route("{bus}/request")]
    public Task<string> Request([FromRoute]string bus, [FromBody]RequestMessageDto messageDto)
    {
        logger.LogInformation("Received: {MessageDto}", messageDto.ToString());
        var messageBus = busManager.GetMessageBusByName(bus);
        var destination = busManager.GetDestinationByName(messageDto.Destination) ?? messageDto.Destination;
        var replyDestination = messageDto.ReplyToType == ReplyTo.DestinationName ? busManager.GetDestinationByName(messageDto.ReplyTo) : destination;
        return messageBus.Request(destination, messageDto.Payload, messageDto.ReplyToType == ReplyTo.TemporaryQueue, replyDestination);
    }
    
    [HttpPost]
    [Route("{bus}/subscribe")]
    public IAsyncEnumerable<string> Subscribe([FromRoute]string bus, [FromBody]SubscribeMessageDto messageDto, CancellationToken token)
    {
        var destination = busManager.GetDestinationByName(messageDto.Destination) ?? messageDto.Destination;
        var messageBus = busManager.GetMessageBusByName(bus);
        return messageBus.Subscribe(destination, messageDto.NumberOfEvents, messageDto.Timeout, token);
    }
}