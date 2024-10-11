using Apache.NMS.RestAPI.Interfaces.Services;

namespace Apache.NMS.RestAPI.Interfaces
{
    public interface IBusManager
    {
        IMessageBus GetMessageBusByName(string name);
        string GetDestinationByName(string name);
    }
}