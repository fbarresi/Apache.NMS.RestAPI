using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Apache.NMS.RestAPI.Interfaces.Extensions;
using Apache.NMS.RestAPI.Interfaces.Services;
using Apache.NMS.RestAPI.Interfaces.Settings;
using Apache.NMS.Util;
using Microsoft.Extensions.Logging;

namespace Apache.NMS.RestAPI.Logic.Services;

public class MessageBus : IMessageBus, IDisposable
{
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            disposables.Dispose();
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
    private readonly Subject<Unit> reconnectSubject = new();
    private readonly CompositeDisposable disposables = new();

    public MessageBus(ILogger<MessageBus> logger, MessageBusSessionSettings settings)
    {
        this.logger = logger;
        this.settings = settings;
    }

    public Task StartAsync()
    {
        CreateConnection();

        reconnectSubject
            .Throttle(TimeSpan.FromSeconds(3))
            .Do(_ => CreateConnection())
            .Subscribe()
            .AddDisposableTo(disposables);

        return Task.FromResult(true);
    }

    private void CreateConnection()
    {
        logger.LogInformation("Starting message bus {Name}...", settings.Name);
        try
        {
            logger.LogInformation("Connect to server: {Server}", settings.ServerUrl);
            var factory = new NMSConnectionFactory(settings.ServerUrl);
            connection = factory.CreateConnection(settings.Username, settings.Password);
            connection.AddDisposableTo(disposables);
            session = connection.CreateSession();
            session.AddDisposableTo(disposables);

            CreateDefaultProducer();
            connection.Start();
            logger.LogInformation("Connection for message bus {Name} started!", settings.Name);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while starting message bus service");
        }
    }

    private void CreateDefaultProducer()
    {
        if (!string.IsNullOrEmpty(settings.DefaultDestination))
        {
            var destination = SessionUtil.GetDestination(session, settings.DefaultDestination);
            logger.LogInformation("Destination: {Destination}", settings.DefaultDestination);

            defaultProducer = session.CreateProducer(destination);
            defaultProducer.DeliveryMode = (MsgDeliveryMode)settings.DeliveryMode;
            defaultProducer.RequestTimeout = settings.RequestTimeout;
            defaultProducer.AddDisposableTo(disposables);
        }
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
            reconnectSubject.OnNext(Unit.Default);
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
            return Task.FromResult(string.Empty);

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while sending JMS message");
            reconnectSubject.OnNext(Unit.Default);
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

    public async IAsyncEnumerable<string> Subscribe(string destination, int numberOfEvents, TimeSpan timeout,
        [EnumeratorCancellation] CancellationToken token)
    {
        using var dest = SessionUtil.GetDestination(session, destination);
        using var consumer = session.CreateConsumer(dest);

        using var internalToken = new CancellationTokenSource(timeout);
        using var compositeToken = CancellationTokenSource.CreateLinkedTokenSource(internalToken.Token, token);
        
        var events = 0;
        do
        {
            var reply = await consumer.ReceiveAsync(settings.ReceiveTimeout);
            if (reply != null)
            {
                events++;
                logger.LogInformation("Received message with ID: {MessageId}", reply.NMSMessageId);
                var textReply = (reply as ITextMessage)?.Text;
                logger.LogInformation("Received message with text: {Reply}", textReply);

                if (!string.IsNullOrEmpty(textReply))
                {
                    yield return textReply;
                }
            }
        } while (!compositeToken.IsCancellationRequested && (events < numberOfEvents || numberOfEvents <= 0));
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