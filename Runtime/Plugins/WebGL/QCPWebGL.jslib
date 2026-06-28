mergeInto(LibraryManager.library, {
  QCP_JS_Connect: function (endpointPtr) {
    var endpoint = UTF8ToString(endpointPtr);
    if (!endpoint || endpoint.length === 0) {
      return -1;
    }
    if (typeof window === "undefined") {
      return -2;
    }
    window.__qcpUnity = window.__qcpUnity || {};
    try {
      var socket = new WebSocket(endpoint);
      socket.binaryType = "arraybuffer";
      window.__qcpUnity.socket = socket;
      window.__qcpUnity.queue = [];
      socket.onmessage = function (event) {
        window.__qcpUnity.queue.push(event.data);
      };
      return 0;
    } catch (e) {
      console.error("[QCP] connect failed", e);
      return -3;
    }
  },

  QCP_JS_Send: function (payloadPtr, length, stream, deadlineMs, priority, latestKey, flags) {
    var state = window.__qcpUnity;
    if (!state || !state.socket) {
      return -1;
    }
    if (state.socket.readyState !== WebSocket.OPEN) {
      return -2;
    }
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
    state.socket.send(out);
    return 0;
  },

  QCP_JS_Close: function () {
    var state = window.__qcpUnity;
    if (state && state.socket) {
      state.socket.close();
      state.socket = null;
    }
    return 0;
  }
});
