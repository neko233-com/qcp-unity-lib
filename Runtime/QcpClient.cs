using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neko233.Qcp.Unity
{
    public sealed class QcpClient : IDisposable
    {
        private readonly IQcpTransport transport;

        public QcpClient(IQcpTransport transport)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public bool IsConnected => transport.IsConnected;

        public static QcpClient CreateDefault()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new QcpClient(new QcpWebGLTransport());
#else
            return new QcpClient(new QcpNativeUdpTransport());
#endif
        }

        public Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            return transport.ConnectAsync(endpoint, cancellationToken);
        }

        public Task SendAsync(byte[] payload, QcpSendOptions options, CancellationToken cancellationToken = default)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            return transport.SendAsync(payload, options, cancellationToken);
        }

        public Task<QcpMessage?> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            return transport.ReceiveAsync(cancellationToken);
        }

        public Task CloseAsync()
        {
            return transport.CloseAsync();
        }

        public void Dispose()
        {
            transport.Dispose();
        }
    }
}
