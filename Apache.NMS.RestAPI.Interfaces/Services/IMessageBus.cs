using System.Threading.Tasks;
using Apache.NMS.RestAPI.Interfaces.DTOs;

namespace Apache.NMS.RestAPI.Interfaces.Services
{
    public interface IMessageBus
    {
        Task Send(string destination, string message);
        Task<string> Request(string destination, string message, bool useTempDestination, string replyDestination);
    }
}