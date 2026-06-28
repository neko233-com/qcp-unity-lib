function qcpBuildPacket(payloadPtr, length, stream, deadlineMs, priority, latestKey, flags) {
  var headerSize = 12;
  var out = new Uint8Array(headerSize + length);
  out[0] = stream & 0xff;
  out[1] = priority & 0xff;
  out[2] = flags & 0xff;
  out[3] = (flags >>> 8) & 0xff;
  out[4] = deadlineMs & 0xff;
  out[5] = (deadlineMs >>> 8) & 0xff;
  out[6] = (deadlineMs >>> 16) & 0xff;
  out[7] = (deadlineMs >>> 24) & 0xff;
  out[8] = latestKey & 0xff;
  out[9] = (latestKey >>> 8) & 0xff;
  out[10] = (latestKey >>> 16) & 0xff;
  out[11] = (latestKey >>> 24) & 0xff;
  out.set(HEAPU8.subarray(payloadPtr, payloadPtr + length), headerSize);
  return out;
}

function qcpParseEndpoint(endpoint) {
  var normalized = endpoint;
  if (normalized.indexOf("qcp://") === 0) {
    normalized = "udp://" + normalized.substring(6);
  }
  var match = normalized.match(/^([a-zA-Z0-9+.-]+):\/\/([^:/]+)(?::(\d+))?/);
  if (!match) {
    return { scheme: "wss", address: endpoint, port: 0, raw: endpoint };
  }
  return {
    scheme: match[1].toLowerCase(),
    address: match[2],
    port: match[3] ? parseInt(match[3], 10) : 0,
    raw: endpoint
  };
}

function qcpCapabilities() {
  var caps = 0;
  if (typeof wx !== "undefined" && typeof wx.createUDPSocket === "function") {
    caps |= 1; // Datagram
    caps |= 2; // RawUdp
    caps |= 4; // LowAllocation
  }
  return caps;
}

mergeInto(LibraryManager.library, {
  QCP_JS_GetCapabilities: function () {
    return qcpCapabilities();
  },

  QCP_JS_Connect: function (endpointPtr) {
    return -100; // QCP-only: standard browser WebGL has no UDP transport.
  },

  QCP_JS_Send: function (payloadPtr, length, stream, deadlineMs, priority, latestKey, flags) {
    return -100; // QCP-only: no WebSocket fallback.
  },

  QCP_JS_Close: function () {
    return 0;
  },

  QCP_WX_IsSupported: function () {
    return typeof wx !== "undefined" ? 1 : 0;
  },

  QCP_WX_GetCapabilities: function () {
    return qcpCapabilities();
  },

  QCP_WX_Connect: function (endpointPtr) {
    var endpoint = UTF8ToString(endpointPtr);
    if (!endpoint || endpoint.length === 0) {
      return -1;
    }
    if (typeof wx === "undefined") {
      return -2;
    }

    var target = qcpParseEndpoint(endpoint);
    window.__qcpUnity = window.__qcpUnity || {};
    window.__qcpUnity.wx = window.__qcpUnity.wx || {};

    if (typeof wx.createUDPSocket === "function" && target.scheme !== "ws" && target.scheme !== "wss") {
      try {
        var udp = wx.createUDPSocket();
        window.__qcpUnity.wx.mode = "udp";
        window.__qcpUnity.wx.socket = udp;
        window.__qcpUnity.wx.address = target.address;
        window.__qcpUnity.wx.port = target.port;
        if (typeof udp.onMessage === "function") {
          udp.onMessage(function (event) {
            window.__qcpUnity.wx.queue = window.__qcpUnity.wx.queue || [];
            window.__qcpUnity.wx.queue.push(event.message);
          });
        }
        if (typeof udp.bind === "function") {
          udp.bind();
        }
        return 0;
      } catch (e) {
        console.error("[QCP] wx UDP unavailable", e);
        return -5;
      }
    }
    return -3; // QCP-only: no UDP, no transport.
  },

  QCP_WX_Send: function (payloadPtr, length, stream, deadlineMs, priority, latestKey, flags) {
    var state = window.__qcpUnity && window.__qcpUnity.wx;
    if (!state || !state.socket) {
      return -1;
    }
    var out = qcpBuildPacket(payloadPtr, length, stream, deadlineMs, priority, latestKey, flags);

    if (state.mode === "udp") {
      if (!state.port) {
        return -2;
      }
      state.socket.send({
        address: state.address,
        port: state.port,
        message: out.buffer
      });
      return 0;
    }

    return -3;
  },

  QCP_WX_Close: function () {
    var state = window.__qcpUnity && window.__qcpUnity.wx;
    if (!state || !state.socket) {
      return 0;
    }
    if (state.mode === "udp" && typeof state.socket.close === "function") {
      state.socket.close();
    }
    state.socket = null;
    return 0;
  }
});
