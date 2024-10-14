using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apache.NMS.RestAPI.Interfaces.DTOs;

namespace Apache.NMS.RestAPI.Interfaces.Services
{
    public interface IMessageBus
    {
        Task Send(string destination, string message);
        Task<string> Request(string destination, string message, bool useTempDestination, string replyDestination);
        IAsyncEnumerable<string> Subscribe(string destination, int numberOfEvents, TimeSpan timeout, CancellationToken token);
    }
}