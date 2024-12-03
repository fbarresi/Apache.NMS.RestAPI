using Apache.NMS.RestAPI.Interfaces;
using Apache.NMS.RestAPI.Interfaces.Services;
using Apache.NMS.RestAPI.Interfaces.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apache.NMS.RestAPI.Logic.Services;

public class MessageBusManagerService : BackgroundService, IBusManager
{
    private readonly ILogger<MessageBusManagerService> logger;
    private readonly ILogger<MessageBus> busLogger;
    private readonly BusManagerSettings settings;
    private readonly Dictionary<string, MessageBus> connections = new();

    public override void Dispose()
    {
        foreach (var connection in connections)
        {
            connection.Value.Dispose();
        }
        connections.Clear();
        base.Dispose();
    }

    public MessageBusManagerService(
        ILogger<MessageBusManagerService> logger,
        ILogger<MessageBus> busLogger,
        BusManagerSettings settings)
    {
        this.logger = logger;
        this.busLogger = busLogger;
        this.settings = settings;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting message bus manager");

        foreach (var setting in settings.BusSettings)
        {
            if (!connections.ContainsKey(setting.Name))
            {
                connections.Add(setting.Name, new MessageBus(busLogger, setting));
            }
        }

        var startTasks = connections.Select(c => c.Value.StartAsync()).ToArray();
        Task.WaitAll(startTasks, cancellationToken);

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        var startTasks = connections.Select(c => c.Value.StopAsync()).ToArray();
        Task.WaitAll(startTasks, cancellationToken);
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(-1, stoppingToken);
    }

    public IMessageBus? GetMessageBusByName(string name)
    {
        var found = connections.TryGetValue(name, out var connection);
        if (!found)
        {
            logger.LogWarning("No connection found for {Name}", name);
            return null;
        }
        return connection;
    }

    public string GetDestinationByName(string name)
    {
        var found = settings.Destinations.TryGetValue(name, out var destination);
        if (!found)
        {
            logger.LogWarning("No destination found for {Name}", name);
            return null;
        }
        return destination;
    }

    public Dictionary<string, bool> States => connections.ToDictionary(pair => pair.Key, pair => pair.Value.Running);
}