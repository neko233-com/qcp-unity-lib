using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Neko233.Qcp.Unity
{
    public sealed class QcpWeChatMiniGameTransport : IQcpTransport
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int QCP_WX_IsSupported();

        [DllImport("__Internal")]
        private static extern int QCP_WX_Connect(string endpoint);

        [DllImport("__Internal")]
        private static extern int QCP_WX_Send(byte[] payload, int length, int stream, int deadlineMs, int priority, uint latestKey, int flags);

        [DllImport("__Internal")]
        private static extern int QCP_WX_Close();

        [DllImport("__Internal")]
        private static extern int QCP_WX_GetCapabilities();
#endif

        public bool IsConnected { get; private set; }

        public static bool IsSupported
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return QCP_WX_IsSupported() == 1 && (QCP_WX_GetCapabilities() & (int)QcpTransportCapabilities.RawUdp) != 0;
#else
                return false;
#endif
            }
        }

        public static QcpPlatformProfile GetPlatformProfile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var capabilities = (QcpTransportCapabilities)QCP_WX_GetCapabilities();
            var preferred = (capabilities & QcpTransportCapabilities.RawUdp) != 0 ? QcpTransportKind.WeChatUdp : QcpTransportKind.Unsupported;
            return new QcpPlatformProfile(QcpPlatformKind.WeChatMiniGame, preferred, capabilities, 1200);
#else
            return new QcpPlatformProfile(QcpPlatformKind.WeChatMiniGame, QcpTransportKind.Unsupported, QcpTransportCapabilities.None, 1200);
#endif
        }

        public Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var rc = QCP_WX_Connect(endpoint);
            IsConnected = rc == 0;
            if (!IsConnected) throw new InvalidOperationException($"QCP WeChat Mini Game connect failed: {rc}");
#else
            throw new PlatformNotSupportedException("QCP WeChat Mini Game transport requires Unity WebGL export running inside WeChat Mini Game with wx.createUDPSocket support.");
#endif
            return Task.CompletedTask;
        }

        public Task SendAsync(ReadOnlyMemory<byte> payload, QcpSendOptions options, CancellationToken cancellationToken = default)
        {
            if (!IsConnected) throw new InvalidOperationException("QCP WeChat Mini Game transport is not connected.");
#if UNITY_WEBGL && !UNITY_EDITOR
            var data = payload.ToArray();
            var rc = QCP_WX_Send(data, data.Length, (int)options.Stream, options.DeadlineMs, (int)options.Priority, options.LatestKey, (int)options.Flags);
            if (rc != 0) throw new InvalidOperationException($"QCP WeChat Mini Game send failed: {rc}");
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
            QCP_WX_Close();
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
