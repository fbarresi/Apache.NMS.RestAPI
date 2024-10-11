using Apache.NMS;
using Apache.NMS.RestAPI.Interfaces.DTOs;
using Apache.NMS.RestAPI.Interfaces.Services;
using Apache.NMS.RestAPI.Interfaces.Settings;
using Apache.NMS.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apache.NMS.RestAPI.Logic.Services;

public class MessageBus : IMessageBus, IDisposable
{
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            connection?.Dispose();
            session?.Dispose();
            defaultProducer?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private readonly ILogger<MessageBus> logger;
    private readonly MessageBusSessionSettings settings;
    private IConnection connection;
    private ISession session;
    private IMessageProducer defaultProducer;
    private readonly TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);

    public MessageBus(ILogger<MessageBus> logger, MessageBusSessionSettings settings)
    {
        this.logger = logger;
        this.settings = settings;
    }

    public Task StartAsync()
    {
        logger.LogInformation("Starting message bus {Name}...", settings.Name);
        try
        {
            logger.LogInformation("Connect to server: {Server}", settings.ServerUrl);
            var factory = new NMSConnectionFactory(settings.ServerUrl);
            connection = factory.CreateConnection(settings.Username, settings.Password);
            session = connection.CreateSession();

            if (!string.IsNullOrEmpty(settings.DefaultDestination))
            {
                var destination = SessionUtil.GetDestination(session, settings.DefaultDestination);
                logger.LogInformation("Destination: {Destination}", settings.DefaultDestination);

                defaultProducer = session.CreateProducer(destination);
                defaultProducer.DeliveryMode = (MsgDeliveryMode)settings.DeliveryMode;
                defaultProducer.RequestTimeout = settings.RequestTimeout;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while starting message bus service");
        }

        return Task.FromResult(true);
    }

    public Task StopAsync()
    {
        defaultProducer?.Close();
        session?.Close();
        connection?.Stop();

        return Task.FromResult(true);
    }

    public Task Send(string destination, string message)
    {
        try
        {
            var request = session.CreateTextMessage(message);
            request.NMSMessageId = Guid.NewGuid().ToString();
            if (destination.Equals(settings.DefaultDestination))
            {
                defaultProducer.Send(request);
            }
            else
            {
                using var messageProducer = GetProducer(destination);
                messageProducer.Send(request);
                messageProducer.Close();
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while sending message to {Destination}", destination);
            throw;
        }
        return Task.FromResult(true);
    }

    public Task<string> Request(string destination, string message, bool useTempDestination, string replyDestination)
    {
        using var replyDest = GetDestination(useTempDestination, replyDestination);
        try
        {
            var request = session.CreateTextMessage(message);
            request.NMSMessageId = Guid.NewGuid().ToString();
            request.NMSReplyTo = replyDest;
            using var consumer = session.CreateConsumer(replyDest);
            
            logger.LogInformation("Sending message with ID: {MessageId} and ReplyTo: {ReplyTo}", request?.NMSMessageId, replyDest);
            defaultProducer.Send(request);
            var reply = consumer.Receive(settings.RequestTimeout);
            logger.LogInformation("Received message with ID: {MessageId}", reply?.NMSMessageId);
            var textReply = (reply as ITextMessage)?.Text;
            logger.LogInformation("Received message with text: {Reply}", textReply);

            if (!string.IsNullOrEmpty(textReply))
            {
                return Task.FromResult(textReply);
            }
            else
            {
                //eventually throw on no reply
            }
            return Task.FromResult<string>(string.Empty);

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while sending JMS message");
            throw;
        }
        finally
        {
            if (replyDest.IsTemporary)
            {
                var temporaryQueue = (replyDest as ITemporaryQueue);
                temporaryQueue?.Delete();
                logger.LogInformation("temp queue {TempQueue} deleted", temporaryQueue?.QueueName);
            }
        }
    }

    private IDestination GetDestination(bool useTemp, string destination)
    {
        if (useTemp)
        {
            return session.CreateTemporaryQueue();
        }
        return SessionUtil.GetDestination(session, destination);
    }
    private IMessageProducer GetProducer(string destination)
    {
        var dest = SessionUtil.GetDestination(session, destination);
        logger.LogInformation("Creating producer for: {Destination}", destination);

        var customProducer = session.CreateProducer(dest);
        customProducer.DeliveryMode = (MsgDeliveryMode)settings.DeliveryMode;
        customProducer.RequestTimeout = settings.RequestTimeout;
        return customProducer;
    }
}