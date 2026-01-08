#region Assembly NetCoreServer, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null
// E:\projects\unity\Pirates\PiratesServer\dependencies\NetCoreServer.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace NetCoreServer {
    public class WebSocket : IWebSocket {
        private readonly IWebSocket _wsHandler;

        public const byte WS_FIN = 128;

        public const byte WS_TEXT = 1;

        public const byte WS_BINARY = 2;

        public const byte WS_CLOSE = 8;

        public const byte WS_PING = 9;

        public const byte WS_PONG = 10;

        internal bool WsHandshaked;

        internal bool WsFrameReceived;

        internal bool WsFinalReceived;

        internal byte WsOpcode;

        internal long WsHeaderSize;

        internal long WsPayloadSize;

        internal readonly object WsReceiveLock = new object();

        internal readonly Buffer WsReceiveFrameBuffer = new Buffer();

        internal readonly Buffer WsReceiveFinalBuffer = new Buffer();

        internal readonly byte[] WsReceiveMask = new byte[4];

        internal readonly object WsSendLock = new object();

        internal readonly Buffer WsSendBuffer = new Buffer();

        internal readonly byte[] WsSendMask = new byte[4];

        internal readonly Random WsRandom = new Random();

        internal readonly byte[] WsNonce = new byte[16];

        public WebSocket(IWebSocket wsHandler) {
            _wsHandler = wsHandler;
            ClearWsBuffers();
            InitWsNonce();
        }

        public bool PerformClientUpgrade(HttpResponse response, Guid id) {
            if (response.Status != 101) {
                return false;
            }

            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            for (int i = 0; i < response.Headers; i++) {
                var (strA, text) = response.Header(i);
                if (string.Compare(strA, "Connection", StringComparison.OrdinalIgnoreCase) == 0) {
                    if (string.Compare(text, "Upgrade", StringComparison.OrdinalIgnoreCase) != 0) {
                        flag = true;
                        _wsHandler.OnWsError("Invalid WebSocket handshaked response: 'Connection' header value must be 'Upgrade'");
                        break;
                    }

                    flag3 = true;
                }
                else if (string.Compare(strA, "Upgrade", StringComparison.OrdinalIgnoreCase) == 0) {
                    if (string.Compare(text, "websocket", StringComparison.OrdinalIgnoreCase) != 0) {
                        flag = true;
                        _wsHandler.OnWsError("Invalid WebSocket handshaked response: 'Upgrade' header value must be 'websocket'");
                        break;
                    }

                    flag4 = true;
                }
                else if (string.Compare(strA, "Sec-WebSocket-Accept", StringComparison.OrdinalIgnoreCase) == 0) {
                    string s = Convert.ToBase64String(WsNonce) + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                    string @string;
                    using (SHA1 sHA = SHA1.Create()) {
                        @string = Encoding.UTF8.GetString(sHA.ComputeHash(Encoding.UTF8.GetBytes(s)));
                    }

                    s = Encoding.UTF8.GetString(Convert.FromBase64String(text));
                    if (string.Compare(s, @string, StringComparison.InvariantCulture) != 0) {
                        flag = true;
                        _wsHandler.OnWsError("Invalid WebSocket handshaked response: 'Sec-WebSocket-Accept' value validation failed");
                        break;
                    }

                    flag2 = true;
                }
            }

            if (!flag2 || !flag3 || !flag4) {
                if (!flag) {
                    _wsHandler.OnWsError("Invalid WebSocket response");
                }

                return false;
            }

            WsHandshaked = true;
            WsRandom.NextBytes(WsSendMask);
            _wsHandler.OnWsConnected(response);
            return true;
        }

        public bool PerformServerUpgrade(HttpRequest request, HttpResponse response) {
            if (request.Method != "GET") {
                return false;
            }

            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            string value = "";
            for (int i = 0; i < request.Headers; i++) {
                var (strA, text) = request.Header(i);
                if (string.Compare(strA, "Connection", StringComparison.OrdinalIgnoreCase) == 0) {
                    if (string.Compare(text, "Upgrade", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(text, "keep-alive, Upgrade", StringComparison.OrdinalIgnoreCase) != 0) {
                        flag = true;
                        response.MakeErrorResponse(400, "Invalid WebSocket handshaked request: 'Connection' header value must be 'Upgrade' or 'keep-alive, Upgrade'");
                        break;
                    }

                    flag2 = true;
                }
                else if (string.Compare(strA, "Upgrade", StringComparison.OrdinalIgnoreCase) == 0) {
                    if (string.Compare(text, "websocket", StringComparison.OrdinalIgnoreCase) != 0) {
                        flag = true;
                        response.MakeErrorResponse(400, "Invalid WebSocket handshaked request: 'Upgrade' header value must be 'websocket'");
                        break;
                    }

                    flag3 = true;
                }
                else if (string.Compare(strA, "Sec-WebSocket-Key", StringComparison.OrdinalIgnoreCase) == 0) {
                    if (string.IsNullOrEmpty(text)) {
                        flag = true;
                        response.MakeErrorResponse(400, "Invalid WebSocket handshaked request: 'Sec-WebSocket-Key' header value must be non empty");
                        break;
                    }

                    string s = text + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                    byte[] inArray;
                    using (SHA1 sHA = SHA1.Create()) {
                        inArray = sHA.ComputeHash(Encoding.UTF8.GetBytes(s));
                    }

                    value = Convert.ToBase64String(inArray);
                    flag4 = true;
                }
                else if (string.Compare(strA, "Sec-WebSocket-Version", StringComparison.OrdinalIgnoreCase) == 0) {
                    if (string.Compare(text, "13", StringComparison.OrdinalIgnoreCase) != 0) {
                        flag = true;
                        response.MakeErrorResponse(400, "Invalid WebSocket handshaked request: 'Sec-WebSocket-Version' header value must be '13'");
                        break;
                    }

                    flag5 = true;
                }
            }

            if (!flag2 && !flag3 && !flag4 && !flag5) {
                return false;
            }

            if (!flag2 || !flag3 || !flag4 || !flag5) {
                if (!flag) {
                    response.MakeErrorResponse(400, "Invalid WebSocket response");
                }

                _wsHandler.SendUpgrade(response);
                return false;
            }

            response.Clear();
            response.SetBegin(101);
            response.SetHeader("Connection", "Upgrade");
            response.SetHeader("Upgrade", "websocket");
            response.SetHeader("Sec-WebSocket-Accept", value);
            response.SetBody();
            if (!_wsHandler.OnWsConnecting(request, response)) {
                return false;
            }

            _wsHandler.SendUpgrade(response);
            WsHandshaked = true;
            Array.Fill(WsSendMask, (byte)0);
            _wsHandler.OnWsConnected(request);
            return true;
        }

        public void PrepareSendFrame(byte opcode, bool mask, ReadOnlySpan<byte> buffer, int status = 0) {
            bool flag = (opcode & 8) == 8 && buffer.Length > 0;
            long num = (flag ? (buffer.Length + 2) : buffer.Length);
            WsSendBuffer.Clear();
            WsSendBuffer.Append(opcode);
            if (num <= 125) {
                WsSendBuffer.Append((byte)(((uint)(int)num & 0xFFu) | (mask ? 128u : 0u)));
            }
            else if (num <= 65535) {
                WsSendBuffer.Append((byte)(0x7Eu | (mask ? 128u : 0u)));
                WsSendBuffer.Append((byte)((num >> 8) & 0xFF));
                WsSendBuffer.Append((byte)(num & 0xFF));
            }
            else {
                WsSendBuffer.Append((byte)(0x7Fu | (mask ? 128u : 0u)));
                for (int num2 = 7; num2 >= 0; num2--) {
                    WsSendBuffer.Append((byte)((num >> 8 * num2) & 0xFF));
                }
            }

            if (mask) {
                WsSendBuffer.Append(WsSendMask);
            }

            long size = WsSendBuffer.Size;
            WsSendBuffer.Resize(WsSendBuffer.Size + num);
            int num3 = 0;
            if (flag) {
                num3 += 2;
                WsSendBuffer.Append((byte)((uint)(status >> 8) & 0xFFu));
                WsSendBuffer.Append((byte)((uint)status & 0xFFu));
            }

            for (int i = num3; i < num; i++) {
                WsSendBuffer.Data[size + i] = (byte)(buffer[i] ^ WsSendMask[i % 4]);
            }
        }

        public void PrepareReceiveFrame(byte[] buffer, long offset, long size) {
            lock (WsReceiveLock) {
                int num = 0;
                if (WsFrameReceived) {
                    WsFrameReceived = false;
                    WsHeaderSize = 0L;
                    WsPayloadSize = 0L;
                    WsReceiveFrameBuffer.Clear();
                    Array.Clear(WsReceiveMask, 0, WsReceiveMask.Length);
                }

                if (WsFinalReceived) {
                    WsFinalReceived = false;
                    WsReceiveFinalBuffer.Clear();
                }

                while (size > 0) {
                    if (WsFrameReceived) {
                        WsFrameReceived = false;
                        WsHeaderSize = 0L;
                        WsPayloadSize = 0L;
                        WsReceiveFrameBuffer.Clear();
                        Array.Clear(WsReceiveMask, 0, WsReceiveMask.Length);
                    }

                    if (WsFinalReceived) {
                        WsFinalReceived = false;
                        WsReceiveFinalBuffer.Clear();
                    }

                    if (WsReceiveFrameBuffer.Size < 2) {
                        long num2 = 0L;
                        while (num2 < 2) {
                            if (size == 0L) {
                                return;
                            }

                            WsReceiveFrameBuffer.Append(buffer[offset + num]);
                            num2++;
                            num++;
                            size--;
                        }
                    }

                    byte b = (byte)(WsReceiveFrameBuffer[0L] & 0xFu);
                    bool flag = ((WsReceiveFrameBuffer[0L] >> 7) & 1) != 0;
                    bool flag2 = ((WsReceiveFrameBuffer[1L] >> 7) & 1) != 0;
                    long num3 = WsReceiveFrameBuffer[1L] & -129;
                    WsOpcode = ((b != 0) ? b : WsOpcode);
                    if (num3 <= 125) {
                        WsHeaderSize = 2 + (flag2 ? 4 : 0);
                        WsPayloadSize = num3;
                    }
                    else {
                        switch (num3) {
                            case 126L:
                                if (WsReceiveFrameBuffer.Size < 4) {
                                    long num5 = 0L;
                                    while (num5 < 2) {
                                        if (size == 0L) {
                                            return;
                                        }

                                        WsReceiveFrameBuffer.Append(buffer[offset + num]);
                                        num5++;
                                        num++;
                                        size--;
                                    }
                                }

                                num3 = (WsReceiveFrameBuffer[2L] << 8) | WsReceiveFrameBuffer[3L];
                                WsHeaderSize = 4 + (flag2 ? 4 : 0);
                                WsPayloadSize = num3;
                                break;
                            case 127L:
                                if (WsReceiveFrameBuffer.Size < 10) {
                                    long num4 = 0L;
                                    while (num4 < 8) {
                                        if (size == 0L) {
                                            return;
                                        }

                                        WsReceiveFrameBuffer.Append(buffer[offset + num]);
                                        num4++;
                                        num++;
                                        size--;
                                    }
                                }

                                num3 = (WsReceiveFrameBuffer[2L] << 24) | (WsReceiveFrameBuffer[3L] << 16) | (WsReceiveFrameBuffer[4L] << 8) | WsReceiveFrameBuffer[5L] | (WsReceiveFrameBuffer[6L] << 24) | (WsReceiveFrameBuffer[7L] << 16) | (WsReceiveFrameBuffer[8L] << 8) | WsReceiveFrameBuffer[9L];
                                WsHeaderSize = 10 + (flag2 ? 4 : 0);
                                WsPayloadSize = num3;
                                break;
                        }
                    }

                    if (flag2 && WsReceiveFrameBuffer.Size < WsHeaderSize) {
                        long num6 = 0L;
                        while (num6 < 4) {
                            if (size == 0L) {
                                return;
                            }

                            WsReceiveFrameBuffer.Append(buffer[offset + num]);
                            WsReceiveMask[num6] = buffer[offset + num];
                            num6++;
                            num++;
                            size--;
                        }
                    }

                    long num7 = WsHeaderSize + WsPayloadSize;
                    long num8 = Math.Min(num7 - WsReceiveFrameBuffer.Size, size);
                    WsReceiveFrameBuffer.Append(buffer[((int)offset + num)..((int)offset + num + (int)num8)]);
                    num += (int)num8;
                    size -= num8;
                    if (WsReceiveFrameBuffer.Size != num7) {
                        continue;
                    }

                    if (flag2) {
                        for (long num9 = 0L; num9 < WsPayloadSize; num9++) {
                            WsReceiveFinalBuffer.Append((byte)(WsReceiveFrameBuffer[WsHeaderSize + num9] ^ WsReceiveMask[num9 % 4]));
                        }
                    }
                    else {
                        WsReceiveFinalBuffer.Append(WsReceiveFrameBuffer.AsSpan().Slice((int)WsHeaderSize, (int)WsPayloadSize));
                    }

                    WsFrameReceived = true;
                    if (!flag) {
                        continue;
                    }

                    WsFinalReceived = true;
                    switch (WsOpcode) {
                        case 9:
                            _wsHandler.OnWsPing(WsReceiveFinalBuffer.Data, 0L, WsReceiveFinalBuffer.Size);
                            break;
                        case 10:
                            _wsHandler.OnWsPong(WsReceiveFinalBuffer.Data, 0L, WsReceiveFinalBuffer.Size);
                            break;
                        case 8: {
                                int num10 = 0;
                                int status = 1000;
                                if (WsReceiveFinalBuffer.Size > 2) {
                                    num10 += 2;
                                    status = (WsReceiveFinalBuffer[0L] << 8) | WsReceiveFinalBuffer[1L];
                                }

                                _wsHandler.OnWsClose(WsReceiveFinalBuffer.Data, num10, WsReceiveFinalBuffer.Size - num10, status);
                                break;
                            }
                        case 1:
                        case 2:
                            _wsHandler.OnWsReceived(WsReceiveFinalBuffer.Data, 0L, WsReceiveFinalBuffer.Size);
                            break;
                    }
                }
            }
        }

        public long RequiredReceiveFrameSize() {
            lock (WsReceiveLock) {
                if (WsFrameReceived) {
                    return 0L;
                }

                if (WsReceiveFrameBuffer.Size < 2) {
                    return 2 - WsReceiveFrameBuffer.Size;
                }

                bool flag = ((WsReceiveFrameBuffer[1L] >> 7) & 1) != 0;
                long num = WsReceiveFrameBuffer[1L] & -129;
                if (num == 126 && WsReceiveFrameBuffer.Size < 4) {
                    return 4 - WsReceiveFrameBuffer.Size;
                }

                if (num == 127 && WsReceiveFrameBuffer.Size < 10) {
                    return 10 - WsReceiveFrameBuffer.Size;
                }

                if (flag && WsReceiveFrameBuffer.Size < WsHeaderSize) {
                    return WsHeaderSize - WsReceiveFrameBuffer.Size;
                }

                return WsHeaderSize + WsPayloadSize - WsReceiveFrameBuffer.Size;
            }
        }

        public void ClearWsBuffers() {
            bool lockTaken = false;
            try {
                Monitor.TryEnter(WsReceiveLock, ref lockTaken);
                if (lockTaken) {
                    WsFrameReceived = false;
                    WsFinalReceived = false;
                    WsHeaderSize = 0L;
                    WsPayloadSize = 0L;
                    WsReceiveFrameBuffer.Clear();
                    WsReceiveFinalBuffer.Clear();
                    Array.Clear(WsReceiveMask, 0, WsReceiveMask.Length);
                }
            }
            finally {
                if (lockTaken) {
                    Monitor.Exit(WsReceiveLock);
                }
            }

            lock (WsSendLock) {
                WsSendBuffer.Clear();
                Array.Clear(WsSendMask, 0, WsSendMask.Length);
            }
        }

        public void InitWsNonce() {
            WsRandom.NextBytes(WsNonce);
        }
    }
#if false // Decompilation log
'168' items in cache
------------------
Resolve: 'System.Runtime, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Runtime.dll'
------------------
Resolve: 'System.Threading, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Threading.dll'
------------------
Resolve: 'System.Collections, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Collections.dll'
------------------
Resolve: 'System.IO.FileSystem.Watcher, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.IO.FileSystem.Watcher, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.IO.FileSystem.Watcher.dll'
------------------
Resolve: 'System.Net.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Primitives, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Net.Primitives.dll'
------------------
Resolve: 'System.Net.Sockets, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Sockets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Net.Sockets.dll'
------------------
Resolve: 'System.Net.Security, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Security, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Net.Security.dll'
------------------
Resolve: 'System.Security.Cryptography, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Security.Cryptography, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Security.Cryptography.dll'
------------------
Resolve: 'System.Collections.Concurrent, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Concurrent, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Collections.Concurrent.dll'
------------------
Resolve: 'System.Web.HttpUtility, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Web.HttpUtility, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Web.HttpUtility.dll'
------------------
Resolve: 'System.Memory, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Memory, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Memory.dll'
------------------
Resolve: 'System.Threading.Thread, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.Thread, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Threading.Thread.dll'
------------------
Resolve: 'System.ComponentModel.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.Primitives, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.ComponentModel.Primitives.dll'
------------------
Resolve: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Runtime.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.InteropServices, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.Unsafe, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.CompilerServices.Unsafe, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.8\ref\net8.0\System.Runtime.CompilerServices.Unsafe.dll'
#endif
}