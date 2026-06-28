# qcp-unity-lib

Unity package for QCP transport integration.

QCP stays at the protocol layer. Game code decides message meaning; this package provides stream semantics, platform transport selection, and WeChat Mini Game UDP bridge hooks.

## Install

Use Unity Package Manager with Git URL:

```text
https://github.com/neko233-com/qcp-unity-lib.git
```

Or add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.neko233.qcp": "https://github.com/neko233-com/qcp-unity-lib.git"
  }
}
```

## Streams

| Stream | Use | Protocol behavior |
|--------|-----|-------------------|
| `Realtime` | movement, transform, AOI snapshots | latest-wins, old state may be dropped |
| `Critical` | combat commands, skill casts, hit events | bounded reliable with deadline |
| `Batch` | chat, inventory, room control, telemetry | reliable ordered delivery |

## Platform Transport

| Platform | Transport |
|----------|-----------|
| Editor / Standalone / Mobile | `QcpNativeUdpTransport` placeholder for native QCP UDP binding |
| Browser WebGL | Unsupported for QCP transport because standard browser WebGL does not expose UDP |
| WeChat Mini Game | `QcpWeChatMiniGameTransport` through `wx.createUDPSocket` in `Runtime/Plugins/WebGL/QCPWebGL.jslib` |

This package is QCP-only. It does not fall back to TCP or WebSocket. If the runtime does not expose datagram UDP, the transport fails fast instead of silently changing protocol behavior.

For WeChat Mini Game packaging, keep the C# API and exported `QCP_WX_*` names stable. The bridge calls `wx.createUDPSocket`; if that API is unavailable on the target runtime, QCP is unsupported on that build target.

## Usage

```csharp
using System.Text;
using Neko233.Qcp.Unity;

var client = QcpClient.CreateDefault();
await client.ConnectAsync("qcp://example.com:7000");

await client.SendAsync(
    Encoding.UTF8.GetBytes("move:1,2,3"),
    new QcpSendOptions
    {
        Stream = QcpStream.Realtime,
        DeadlineMs = 16,
        Priority = QcpPriority.Normal
    });
```

## Production Notes

- Keep gameplay schema above QCP. This package sends bytes.
- Use `Realtime` for high-frequency state; do not enqueue old movement packets.
- Use `Critical` for combat commands and hit events with a deadline.
- Use `Batch` for reliable non-frame-critical data.
- WeChat Mini Game must expose `wx.createUDPSocket`; there is no TCP / WebSocket fallback.
- Keep payloads below the platform profile `MaxPayloadBytes` value so QCP can avoid fragmentation on mobile networks.
