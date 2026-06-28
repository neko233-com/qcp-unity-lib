using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Neko233.Qcp.Unity
{
    public sealed class QcpWebGLTransport : IQcpTransport
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int QCP_JS_Connect(string endpoint);

        [DllImport("__Internal")]
        private static extern int QCP_JS_Send(byte[] payload, int length, int stream, int deadlineMs, int priority, uint latestKey, int flags);

        [DllImport("__Internal")]
        private static extern int QCP_JS_Close();

        [DllImport("__Internal")]
        private static extern int QCP_JS_GetCapabilities();
#endif

        public bool IsConnected { get; private set; }

        public static QcpPlatformProfile GetPlatformProfile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var capabilities = (QcpTransportCapabilities)QCP_JS_GetCapabilities();
            return new QcpPlatformProfile(QcpPlatformKind.WebGL, QcpTransportKind.Unsupported, capabilities, 1200);
#else
            return new QcpPlatformProfile(QcpPlatformKind.WebGL, QcpTransportKind.Unsupported, QcpTransportCapabilities.None, 1200);
#endif
        }

        public Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            throw new PlatformNotSupportedException("QCP requires datagram UDP. Standard browser WebGL does not expose UDP; use WeChat Mini Game UDP or a native platform.");
#else
            throw new PlatformNotSupportedException("QCP WebGL transport is only a compile-time placeholder. Use QcpWeChatMiniGameTransport for WeChat Mini Game UDP.");
#endif
        }

        public Task SendAsync(ReadOnlyMemory<byte> payload, QcpSendOptions options, CancellationToken cancellationToken = default)
        {
            if (!IsConnected) throw new InvalidOperationException("QCP WebGL transport is not connected.");
#if UNITY_WEBGL && !UNITY_EDITOR
            var data = payload.ToArray();
            var rc = QCP_JS_Send(data, data.Length, (int)options.Stream, options.DeadlineMs, (int)options.Priority, options.LatestKey, (int)options.Flags);
            if (rc != 0) throw new InvalidOperationException($"QCP WebGL send failed: {rc}");
#endif
            return Task.CompletedTask;
        }

        public Task<QcpMessage?> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<QcpMessage?>(null);
        }

        public Task CloseAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            QCP_JS_Close();
#endif
            IsConnected = false;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            IsConnected = false;
        }
    }
}
