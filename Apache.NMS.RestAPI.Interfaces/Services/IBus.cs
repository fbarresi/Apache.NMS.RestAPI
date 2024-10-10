using System.Threading.Tasks;

namespace Apache.NMS.RestAPI.Interfaces.Services
{
    public interface IBus
    {
        Task<string> SendAsync(string message);
    }
}