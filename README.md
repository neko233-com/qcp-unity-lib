# qcp-unity-lib

Unity package for QCP transport integration.

QCP stays at the protocol layer. Game code decides message meaning; this package provides stream semantics, platform transport selection, and WebGL / WeChat Mini Game bridge hooks.

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
| WebGL | `QcpWebGLTransport` through `Runtime/Plugins/WebGL/QCPWebGL.jslib` |
| WeChat Mini Game | same JS bridge shape; replace the JavaScript internals with WX socket APIs when packaging |

WebGL and WeChat Mini Game builds cannot use raw UDP. The `.jslib` bridge is intentionally isolated behind `IQcpTransport`, so the protocol-facing C# API does not change when the platform bridge is swapped.

For WeChat Mini Game packaging, keep the C# API and exported `QCP_JS_*` names stable, then swap the body of `QCPWebGL.jslib` to call the platform socket implementation provided by the WeChat Unity adapter.

## Usage

```csharp
using System.Text;
using Neko233.Qcp.Unity;

var client = QcpClient.CreateDefault();
await client.ConnectAsync("wss://example.com/qcp");

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
- WebGL / WeChat transport code must be selected at build time through Unity plugin import settings.
