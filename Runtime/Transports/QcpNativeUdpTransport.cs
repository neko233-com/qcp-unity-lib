using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neko233.Qcp.Unity
{
    public sealed class QcpNativeUdpTransport : IQcpTransport
    {
        public bool IsConnected { get; private set; }

        public Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            // Native UDP binding will plug in here. Keep API stable for Unity callers.
            IsConnected = true;
            return Task.CompletedTask;
        }

        public Task SendAsync(ReadOnlyMemory<byte> payload, QcpSendOptions options, CancellationToken cancellationToken = default)
        {
            if (!IsConnected) throw new InvalidOperationException("QCP transport is not connected.");
            return Task.CompletedTask;
        }

        public Task<QcpMessage?> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<QcpMessage?>(null);
        }

        public Task CloseAsync()
        {
            IsConnected = false;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            IsConnected = false;
        }
    }
}
