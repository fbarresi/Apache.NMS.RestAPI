using Apache.NMS;
using Apache.NMS.RestAPI.Interfaces.Services;
using Apache.NMS.RestAPI.Interfaces.Settings;
using Apache.NMS.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apache.NMS.RestAPI.Logic.Services;

public class MessageBusService : BackgroundService, IBus
{
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            connection?.Dispose();
            session?.Dispose();
            producer?.Dispose();
        }
    }

    public sealed override void Dispose()
    {
        Dispose(true);
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private readonly ILogger<MessageBusService> logger;
    private readonly MessageBusSettings settings;
    private IConnection connection;
    private ISession session;
    private IMessageProducer producer;
    private readonly TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);

    public MessageBusService(ILogger<MessageBusService> logger, MessageBusSettings settings)
    {
        this.logger = logger;
        this.settings = settings;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting message bus service...");
        try
        {
            logger.LogInformation("Connect to server: {server}", settings.ServerUrl);
            var factory = new NMSConnectionFactory(settings.ServerUrl);
            connection = factory.CreateConnection(settings.Username, settings.Password);
            session = connection.CreateSession();
            
            var destination = SessionUtil.GetDestination(session, settings.Destination);
            logger.LogInformation("Destination: {destination}", settings.Destination);

            producer = session.CreateProducer(destination);
            producer.DeliveryMode = MsgDeliveryMode.Persistent;
            producer.RequestTimeout = receiveTimeout;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while starting message bus service.");
        }
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        producer?.Close();
        session.Close();
        connection?.Stop();

        return base.StopAsync(cancellationToken);
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Delay(-1, stoppingToken);
    }

    public Task<string> SendAsync(string message)
    {
        using var temporaryQueue = session.CreateTemporaryQueue();
        try
        {
            var request = session.CreateTextMessage(message);
            request.NMSMessageId = Guid.NewGuid().ToString();
            request.NMSReplyTo = temporaryQueue;
            using var consumer = session.CreateConsumer(temporaryQueue);
            
            logger.LogInformation("Sending message with ID: {messageId} and ReplyTo: {replyTo}", request?.NMSMessageId, temporaryQueue.QueueName);
            producer.Send(request);
            var reply = consumer.Receive(receiveTimeout);
            logger.LogInformation("Received message with ID: {messageId}", reply?.NMSMessageId);
            var textReply = (reply as ITextMessage)?.Text;
            logger.LogInformation("Received message with text: {reply}", textReply);

            if (!string.IsNullOrEmpty(textReply))
            {
                return Task.FromResult(textReply);
            }
            else
            {
                //todo: eventually throw on no reply
            }
            return Task.FromResult<string>(string.Empty);

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while sending JMS message.");
            throw;
        }
        finally
        {
            temporaryQueue?.Delete();
            logger.LogInformation("temp queue {tempQueue} deleted.", temporaryQueue?.QueueName);
        }
    }
}