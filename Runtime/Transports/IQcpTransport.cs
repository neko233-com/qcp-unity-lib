using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neko233.Qcp.Unity
{
    public interface IQcpTransport : IDisposable
    {
        bool IsConnected { get; }
        Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default);
        Task SendAsync(ReadOnlyMemory<byte> payload, QcpSendOptions options, CancellationToken cancellationToken = default);
        Task<QcpMessage?> ReceiveAsync(CancellationToken cancellationToken = default);
        Task CloseAsync();
    }
}
