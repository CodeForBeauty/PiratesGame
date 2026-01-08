#region Assembly NetCoreServer, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null
// E:\projects\unity\Pirates\PiratesServer\dependencies\NetCoreServer.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Net;

namespace NetCoreServer {
    public class HttpClient : TcpClient {
        public HttpRequest Request { get; protected set; }

        protected HttpResponse Response { get; set; }

        public HttpClient(IPAddress address, int port)
            : base(address, port) {
            Request = new HttpRequest();
            Response = new HttpResponse();
        }

        public HttpClient(string address, int port)
            : base(address, port) {
            Request = new HttpRequest();
            Response = new HttpResponse();
        }

        public HttpClient(DnsEndPoint endpoint)
            : base(endpoint) {
            Request = new HttpRequest();
            Response = new HttpResponse();
        }

        public HttpClient(IPEndPoint endpoint)
            : base(endpoint) {
            Request = new HttpRequest();
            Response = new HttpResponse();
        }

        public long SendRequest() {
            return SendRequest(Request);
        }

        public long SendRequest(HttpRequest request) {
            return Send(request.Cache.Data, request.Cache.Offset, request.Cache.Size);
        }

        public long SendRequestBody(string body) {
            return Send(body);
        }

        public long SendRequestBody(ReadOnlySpan<char> body) {
            return Send(body);
        }

        public long SendRequestBody(byte[] buffer) {
            return Send(buffer);
        }

        public long SendRequestBody(byte[] buffer, long offset, long size) {
            return Send(buffer, offset, size);
        }

        public long SendRequestBody(ReadOnlySpan<byte> buffer) {
            return Send(buffer);
        }

        public bool SendRequestAsync() {
            return SendRequestAsync(Request);
        }

        public bool SendRequestAsync(HttpRequest request) {
            return SendAsync(request.Cache.Data, request.Cache.Offset, request.Cache.Size);
        }

        public bool SendRequestBodyAsync(string body) {
            return SendAsync(body);
        }

        public bool SendRequestBodyAsync(ReadOnlySpan<char> body) {
            return SendAsync(body);
        }

        public bool SendRequestBodyAsync(byte[] buffer) {
            return SendAsync(buffer);
        }

        public bool SendRequestBodyAsync(byte[] buffer, long offset, long size) {
            return SendAsync(buffer, offset, size);
        }

        public bool SendRequestBodyAsync(ReadOnlySpan<byte> buffer) {
            return SendAsync(buffer);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size) {
            if (Response.IsPendingHeader()) {
                if (Response.ReceiveHeader(buffer, (int)offset, (int)size)) {
                    OnReceivedResponseHeader(Response);
                }

                size = 0L;
            }

            if (Response.IsErrorSet) {
                OnReceivedResponseError(Response, "Invalid HTTP response!");
                Response.Clear();
                Disconnect();
            }
            else if (Response.ReceiveBody(buffer, (int)offset, (int)size)) {
                OnReceivedResponse(Response);
                Response.Clear();
            }
            else if (Response.IsErrorSet) {
                OnReceivedResponseError(Response, "Invalid HTTP response!");
                Response.Clear();
                Disconnect();
            }
        }

        protected override void OnDisconnected() {
            if (Response.IsPendingBody()) {
                OnReceivedResponse(Response);
                Response.Clear();
            }
        }

        protected virtual void OnReceivedResponseHeader(HttpResponse response) {
        }

        protected virtual void OnReceivedResponse(HttpResponse response) {
        }

        protected virtual void OnReceivedResponseError(HttpResponse response, string error) {
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