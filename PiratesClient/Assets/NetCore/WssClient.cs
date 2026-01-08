#region Assembly NetCoreServer, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null
// E:\projects\unity\Pirates\PiratesServer\dependencies\NetCoreServer.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetCoreServer {
    public class WssClient : HttpsClient, IWebSocket {
        internal readonly WebSocket WebSocket;

        private bool _syncConnect;

        public byte[] WsNonce => WebSocket.WsNonce;

        public WssClient(SslContext context, IPAddress address, int port)
            : base(context, address, port) {
            WebSocket = new WebSocket(this);
        }

        public WssClient(SslContext context, string address, int port)
            : base(context, address, port) {
            WebSocket = new WebSocket(this);
        }

        public WssClient(SslContext context, DnsEndPoint endpoint)
            : base(context, endpoint) {
            WebSocket = new WebSocket(this);
        }

        public WssClient(SslContext context, IPEndPoint endpoint)
            : base(context, endpoint) {
            WebSocket = new WebSocket(this);
        }

        public override bool Connect() {
            _syncConnect = true;
            return base.Connect();
        }

        public override bool ConnectAsync() {
            _syncConnect = false;
            return base.ConnectAsync();
        }

        public virtual bool Close(int status) {
            SendClose(status, Span<byte>.Empty);
            base.Disconnect();
            return true;
        }

        public virtual bool CloseAsync(int status) {
            SendCloseAsync(status, Span<byte>.Empty);
            base.DisconnectAsync();
            return true;
        }

        public long SendText(string text) {
            return SendText(Encoding.UTF8.GetBytes(text));
        }

        public long SendText(ReadOnlySpan<char> text) {
            return SendText(Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public long SendText(byte[] buffer) {
            return SendText(buffer.AsSpan());
        }

        public long SendText(byte[] buffer, long offset, long size) {
            return SendText(buffer.AsSpan((int)offset, (int)size));
        }

        public long SendText(ReadOnlySpan<byte> buffer) {
            lock (WebSocket.WsSendLock) {
                WebSocket.PrepareSendFrame(129, mask: true, buffer);
                return base.Send(WebSocket.WsSendBuffer.AsSpan());
            }
        }

        public bool SendTextAsync(string text) {
            return SendTextAsync(Encoding.UTF8.GetBytes(text));
        }

        public bool SendTextAsync(ReadOnlySpan<char> text) {
            return SendTextAsync(Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public bool SendTextAsync(byte[] buffer) {
            return SendTextAsync(buffer.AsSpan());
        }

        public bool SendTextAsync(byte[] buffer, long offset, long size) {
            return SendTextAsync(buffer.AsSpan((int)offset, (int)size));
        }

        public bool SendTextAsync(ReadOnlySpan<byte> buffer) {
            lock (WebSocket.WsSendLock) {
                WebSocket.PrepareSendFrame(129, mask: true, buffer);
                return base.SendAsync(WebSocket.WsSendBuffer.AsSpan());
            }
        }

        public long SendBinary(string text) {
            return SendBinary(Encoding.UTF8.GetBytes(text));
        }

        public long SendBinary(ReadOnlySpan<char> text) {
            return SendBinary(Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public long SendBinary(byte[] buffer) {
            return SendBinary(buffer.AsSpan());
        }

        public long SendBinary(byte[] buffer, long offset, long size) {
            return SendBinary(buffer.AsSpan((int)offset, (int)size));
        }

        public long SendBinary(ReadOnlySpan<byte> buffer) {
            lock (WebSocket.WsSendLock) {
                WebSocket.PrepareSendFrame(130, mask: true, buffer);
                return base.Send(WebSocket.WsSendBuffer.AsSpan());
            }
        }

        public bool SendBinaryAsync(string text) {
            return SendBinaryAsync(Encoding.UTF8.GetBytes(text));
        }

        public bool SendBinaryAsync(ReadOnlySpan<char> text) {
            return SendBinaryAsync(Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public bool SendBinaryAsync(byte[] buffer) {
            return SendBinaryAsync(buffer.AsSpan());
        }

        public bool SendBinaryAsync(byte[] buffer, long offset, long size) {
            return SendBinaryAsync(buffer.AsSpan((int)offset, (int)size));
        }

        public bool SendBinaryAsync(ReadOnlySpan<byte> buffer) {
            lock (WebSocket.WsSendLock) {
                WebSocket.PrepareSendFrame(130, mask: true, buffer);
                return base.SendAsync(WebSocket.WsSendBuffer.AsSpan());
            }
        }

        public long SendClose(int status, string text) {
            return SendClose(status, Encoding.UTF8.GetBytes(text));
        }

        public long SendClose(int status, ReadOnlySpan<char> text) {
            return SendClose(status, Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public long SendClose(int status, byte[] buffer) {
            return SendClose(status, buffer.AsSpan());
        }

        public long SendClose(int status, byte[] buffer, long offset, long size) {
            return SendClose(status, buffer.AsSpan((int)offset, (int)size));
        }

        public long SendClose(int status, ReadOnlySpan<byte> buffer) {
            lock (WebSocket.WsSendLock) {
                WebSocket.PrepareSendFrame(136, mask: true, buffer, status);
                return base.Send(WebSocket.WsSendBuffer.AsSpan());
            }
        }

        public bool SendCloseAsync(int status, string text) {
            return SendCloseAsync(status, Encoding.UTF8.GetBytes(text));
        }

        public bool SendCloseAsync(int status, ReadOnlySpan<char> text) {
            return SendCloseAsync(status, Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public bool SendCloseAsync(int status, byte[] buffer) {
            return SendCloseAsync(status, buffer.AsSpan());
        }

        public bool SendCloseAsync(int status, byte[] buffer, long offset, long size) {
            return SendCloseAsync(status, buffer.AsSpan((int)offset, (int)size));
        }

        public bool SendCloseAsync(int status, ReadOnlySpan<byte> buffer) {
            lock (WebSocket.WsSendLock) {
                WebSocket.PrepareSendFrame(136, mask: true, buffer, status);
                return base.SendAsync(WebSocket.WsSendBuffer.AsSpan());
            }
        }

        public long SendPing(string text) {
            return SendPing(Encoding.UTF8.GetBytes(text));
        }

        public long SendPing(ReadOnlySpan<char> text) {
            return SendPing(Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public long SendPing(byte[] buffer) {
            return SendPing(buffer.AsSpan());
        }

        public long SendPing(byte[] buffer, long offset, long size) {
            return SendPing(buffer.AsSpan((int)offset, (int)size));
        }

        public long SendPing(ReadOnlySpan<byte> buffer) {
            lock (WebSocket.WsSendLock) {
                WebSocket.PrepareSendFrame(137, mask: true, buffer);
                return base.Send(WebSocket.WsSendBuffer.AsSpan());
            }
        }

        public bool SendPingAsync(string text) {
            return SendPingAsync(Encoding.UTF8.GetBytes(text));
        }

        public bool SendPingAsync(ReadOnlySpan<char> text) {
            return SendPingAsync(Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public bool SendPingAsync(byte[] buffer) {
            return SendPingAsync(buffer.AsSpan());
        }

        public bool SendPingAsync(byte[] buffer, long offset, long size) {
            return SendPingAsync(buffer.AsSpan((int)offset, (int)size));
        }

        public bool SendPingAsync(ReadOnlySpan<byte> buffer) {
            lock (WebSocket.WsSendLock) {
                WebSocket.PrepareSendFrame(137, mask: true, buffer);
                return base.SendAsync(WebSocket.WsSendBuffer.AsSpan());
            }
        }

        public long SendPong(string text) {
            return SendPong(Encoding.UTF8.GetBytes(text));
        }

        public long SendPong(ReadOnlySpan<char> text) {
            return SendPong(Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public long SendPong(byte[] buffer) {
            return SendPong(buffer.AsSpan());
        }

        public long SendPong(byte[] buffer, long offset, long size) {
            return SendPong(buffer.AsSpan((int)offset, (int)size));
        }

        public long SendPong(ReadOnlySpan<byte> buffer) {
            lock (WebSocket.WsSendLock) {
                WebSocket.PrepareSendFrame(138, mask: true, buffer);
                return base.Send(WebSocket.WsSendBuffer.AsSpan());
            }
        }

        public bool SendPongAsync(string text) {
            return SendPongAsync(Encoding.UTF8.GetBytes(text));
        }

        public bool SendPongAsync(ReadOnlySpan<char> text) {
            return SendPongAsync(Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public bool SendPongAsync(byte[] buffer) {
            return SendPongAsync(buffer.AsSpan());
        }

        public bool SendPongAsync(byte[] buffer, long offset, long size) {
            return SendPongAsync(buffer.AsSpan((int)offset, (int)size));
        }

        public bool SendPongAsync(ReadOnlySpan<byte> buffer) {
            lock (WebSocket.WsSendLock) {
                WebSocket.PrepareSendFrame(138, mask: true, buffer);
                return base.SendAsync(WebSocket.WsSendBuffer.AsSpan());
            }
        }

        public string ReceiveText() {
            Buffer buffer = new Buffer();
            if (!WebSocket.WsHandshaked) {
                return buffer.ExtractString(0L, buffer.Data.Length);
            }

            Buffer buffer2 = new Buffer();
            while (!WebSocket.WsFinalReceived) {
                while (!WebSocket.WsFrameReceived) {
                    long num = WebSocket.RequiredReceiveFrameSize();
                    buffer2.Resize(num);
                    long num2 = (int)base.Receive(buffer2.Data, 0L, num);
                    if (num2 != num) {
                        return buffer.ExtractString(0L, buffer.Data.Length);
                    }

                    WebSocket.PrepareReceiveFrame(buffer2.Data, 0L, num2);
                }

                if (!WebSocket.WsFinalReceived) {
                    WebSocket.PrepareReceiveFrame(null, 0L, 0L);
                }
            }

            buffer.Append(WebSocket.WsReceiveFinalBuffer);
            WebSocket.PrepareReceiveFrame(null, 0L, 0L);
            return buffer.ExtractString(0L, buffer.Data.Length);
        }

        public Buffer ReceiveBinary() {
            Buffer buffer = new Buffer();
            if (!WebSocket.WsHandshaked) {
                return buffer;
            }

            Buffer buffer2 = new Buffer();
            while (!WebSocket.WsFinalReceived) {
                while (!WebSocket.WsFrameReceived) {
                    long num = WebSocket.RequiredReceiveFrameSize();
                    buffer2.Resize(num);
                    long num2 = (int)base.Receive(buffer2.Data, 0L, num);
                    if (num2 != num) {
                        return buffer;
                    }

                    WebSocket.PrepareReceiveFrame(buffer2.Data, 0L, num2);
                }

                if (!WebSocket.WsFinalReceived) {
                    WebSocket.PrepareReceiveFrame(null, 0L, 0L);
                }
            }

            buffer.Append(WebSocket.WsReceiveFinalBuffer);
            WebSocket.PrepareReceiveFrame(null, 0L, 0L);
            return buffer;
        }

        protected override void OnHandshaked() {
            WebSocket.ClearWsBuffers();
            OnWsConnecting(base.Request);
            if (_syncConnect) {
                SendRequest(base.Request);
            }
            else {
                SendRequestAsync(base.Request);
            }
        }

        protected override void OnDisconnecting() {
            if (WebSocket.WsHandshaked) {
                OnWsDisconnecting();
            }
        }

        protected override void OnDisconnected() {
            if (WebSocket.WsHandshaked) {
                WebSocket.WsHandshaked = false;
                OnWsDisconnected();
            }

            base.Request.Clear();
            base.Response.Clear();
            WebSocket.ClearWsBuffers();
            WebSocket.InitWsNonce();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size) {
            if (WebSocket.WsHandshaked) {
                WebSocket.PrepareReceiveFrame(buffer, offset, size);
            }
            else {
                base.OnReceived(buffer, offset, size);
            }
        }

        protected override void OnReceivedResponseHeader(HttpResponse response) {
            if (!WebSocket.WsHandshaked && !WebSocket.PerformClientUpgrade(response, base.Id)) {
                base.OnReceivedResponseHeader(response);
            }
        }

        protected override void OnReceivedResponse(HttpResponse response) {
            if (WebSocket.WsHandshaked) {
                string body = base.Response.Body;
                byte[] bytes = Encoding.UTF8.GetBytes(body);
                WebSocket.PrepareReceiveFrame(bytes, 0L, bytes.Length);
            }
            else {
                base.OnReceivedResponse(response);
            }
        }

        protected override void OnReceivedResponseError(HttpResponse response, string error) {
            if (WebSocket.WsHandshaked) {
                OnError(SocketError.Success);
            }
            else {
                base.OnReceivedResponseError(response, error);
            }
        }

        public virtual void OnWsConnecting(HttpRequest request) {
        }

        public virtual void OnWsConnected(HttpResponse response) {
        }

        public virtual bool OnWsConnecting(HttpRequest request, HttpResponse response) {
            return true;
        }

        public virtual void OnWsConnected(HttpRequest request) {
        }

        public virtual void OnWsDisconnecting() {
        }

        public virtual void OnWsDisconnected() {
        }

        public virtual void OnWsReceived(byte[] buffer, long offset, long size) {
        }

        public virtual void OnWsClose(byte[] buffer, long offset, long size, int status = 1000) {
            CloseAsync(status);
        }

        public virtual void OnWsPing(byte[] buffer, long offset, long size) {
            SendPongAsync(buffer, offset, size);
        }

        public virtual void OnWsPong(byte[] buffer, long offset, long size) {
        }

        public virtual void OnWsError(string error) {
            OnError(SocketError.SocketError);
        }

        public virtual void OnWsError(SocketError error) {
            OnError(error);
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