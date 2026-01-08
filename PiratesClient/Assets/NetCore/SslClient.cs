#region Assembly NetCoreServer, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null
// E:\projects\unity\Pirates\PiratesServer\dependencies\NetCoreServer.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace NetCoreServer {
    public class SslClient : IDisposable {
        private bool _disconnecting;

        private SocketAsyncEventArgs _connectEventArg;

        private SslStream _sslStream;

        private Guid? _sslStreamId;

        private bool _receiving;

        private Buffer _receiveBuffer;

        private readonly object _sendLock = new object();

        private bool _sending;

        private Buffer _sendBufferMain;

        private Buffer _sendBufferFlush;

        private long _sendBufferFlushOffset;

        public Guid Id { get; }

        public string Address { get; }

        public int Port { get; }

        public SslContext Context { get; }

        public EndPoint Endpoint { get; private set; }

        public Socket Socket { get; private set; }

        public long BytesPending { get; private set; }

        public long BytesSending { get; private set; }

        public long BytesSent { get; private set; }

        public long BytesReceived { get; private set; }

        public bool OptionDualMode { get; set; }

        public bool OptionKeepAlive { get; set; }

        public int OptionTcpKeepAliveTime { get; set; } = -1;


        public int OptionTcpKeepAliveInterval { get; set; } = -1;


        public int OptionTcpKeepAliveRetryCount { get; set; } = -1;


        public bool OptionNoDelay { get; set; }

        public int OptionReceiveBufferLimit { get; set; }

        public int OptionReceiveBufferSize { get; set; } = 8192;


        public int OptionSendBufferLimit { get; set; }

        public int OptionSendBufferSize { get; set; } = 8192;


        public bool IsConnecting { get; private set; }

        public bool IsConnected { get; private set; }

        public bool IsHandshaking { get; private set; }

        public bool IsHandshaked { get; private set; }

        public bool IsDisposed { get; private set; }

        public bool IsSocketDisposed { get; private set; } = true;


        public SslClient(SslContext context, IPAddress address, int port)
            : this(context, new IPEndPoint(address, port)) {
        }

        public SslClient(SslContext context, string address, int port)
            : this(context, new IPEndPoint(IPAddress.Parse(address), port)) {
        }

        public SslClient(SslContext context, DnsEndPoint endpoint)
            : this(context, endpoint, endpoint.Host, endpoint.Port) {
        }

        public SslClient(SslContext context, IPEndPoint endpoint)
            : this(context, endpoint, endpoint.Address.ToString(), endpoint.Port) {
        }

        private SslClient(SslContext context, EndPoint endpoint, string address, int port) {
            Id = Guid.NewGuid();
            Address = address;
            Port = port;
            Context = context;
            Endpoint = endpoint;
        }

        protected virtual Socket CreateSocket() {
            return new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public virtual bool Connect() {
            if (IsConnected || IsHandshaked || IsConnecting || IsHandshaking) {
                return false;
            }

            _receiveBuffer = new Buffer();
            _sendBufferMain = new Buffer();
            _sendBufferFlush = new Buffer();
            _connectEventArg = new SocketAsyncEventArgs();
            _connectEventArg.RemoteEndPoint = Endpoint;
            _connectEventArg.Completed += OnAsyncCompleted;
            Socket = CreateSocket();
            IsSocketDisposed = false;
            if (Socket.AddressFamily == AddressFamily.InterNetworkV6) {
                Socket.DualMode = OptionDualMode;
            }

            OnConnecting();
            try {
                Socket.Connect(Endpoint);
            }
            catch (SocketException ex) {
                SendError(ex.SocketErrorCode);
                _connectEventArg.Completed -= OnAsyncCompleted;
                OnDisconnecting();
                Socket.Close();
                Socket.Dispose();
                _connectEventArg.Dispose();
                OnDisconnected();
                return false;
            }

            if (OptionKeepAlive) {
                Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, optionValue: true);
            }

            if (OptionTcpKeepAliveTime >= 0) {
                Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, OptionTcpKeepAliveTime);
            }

            if (OptionTcpKeepAliveInterval >= 0) {
                Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.BlockSource, OptionTcpKeepAliveInterval);
            }

            if (OptionTcpKeepAliveRetryCount >= 0) {
                Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.DontRoute, OptionTcpKeepAliveRetryCount);
            }

            if (OptionNoDelay) {
                Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, optionValue: true);
            }

            _receiveBuffer.Reserve(OptionReceiveBufferSize);
            _sendBufferMain.Reserve(OptionSendBufferSize);
            _sendBufferFlush.Reserve(OptionSendBufferSize);
            BytesPending = 0L;
            BytesSending = 0L;
            BytesSent = 0L;
            BytesReceived = 0L;
            IsConnected = true;
            OnConnected();
            try {
                _sslStreamId = Guid.NewGuid();
                _sslStream = ((Context.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(Socket, ownsSocket: false), leaveInnerStreamOpen: false, Context.CertificateValidationCallback) : new SslStream(new NetworkStream(Socket, ownsSocket: false), leaveInnerStreamOpen: false));
                OnHandshaking();
                if (Context.Certificates != null) {
                    _sslStream.AuthenticateAsClient(Address, Context.Certificates, Context.Protocols, checkCertificateRevocation: true);
                }
                else if (Context.Certificate != null) {
                    _sslStream.AuthenticateAsClient(Address, new X509CertificateCollection(new X509Certificate[1] { Context.Certificate }), Context.Protocols, checkCertificateRevocation: true);
                }
                else {
                    _sslStream.AuthenticateAsClient(Address);
                }
            }
            catch (Exception) {
                SendError(SocketError.NotConnected);
                DisconnectAsync();
                return false;
            }

            IsHandshaked = true;
            OnHandshaked();
            if (_sendBufferMain.IsEmpty) {
                OnEmpty();
            }

            return true;
        }

        public virtual bool Disconnect() {
            if (!IsConnected && !IsConnecting) {
                return false;
            }

            if (IsConnecting) {
                Socket.CancelConnectAsync(_connectEventArg);
            }

            if (_disconnecting) {
                return false;
            }

            IsConnecting = false;
            IsHandshaking = false;
            _disconnecting = true;
            _connectEventArg.Completed -= OnAsyncCompleted;
            OnDisconnecting();
            try {
                try {
                    _sslStream.ShutdownAsync().Wait();
                }
                catch (Exception) {
                }

                _sslStream.Dispose();
                _sslStreamId = null;
                try {
                    Socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException) {
                }

                Socket.Close();
                Socket.Dispose();
                _connectEventArg.Dispose();
                IsSocketDisposed = true;
            }
            catch (ObjectDisposedException) {
            }

            IsHandshaked = false;
            IsConnected = false;
            _receiving = false;
            _sending = false;
            ClearBuffers();
            OnDisconnected();
            _disconnecting = false;
            return true;
        }

        public virtual bool Reconnect() {
            if (!Disconnect()) {
                return false;
            }

            return Connect();
        }

        public virtual bool ConnectAsync() {
            if (IsConnected || IsHandshaked || IsConnecting || IsHandshaking) {
                return false;
            }

            _receiveBuffer = new Buffer();
            _sendBufferMain = new Buffer();
            _sendBufferFlush = new Buffer();
            _connectEventArg = new SocketAsyncEventArgs();
            _connectEventArg.RemoteEndPoint = Endpoint;
            _connectEventArg.Completed += OnAsyncCompleted;
            Socket = CreateSocket();
            IsSocketDisposed = false;
            if (Socket.AddressFamily == AddressFamily.InterNetworkV6) {
                Socket.DualMode = OptionDualMode;
            }

            IsConnecting = true;
            OnConnecting();
            if (!Socket.ConnectAsync(_connectEventArg)) {
                ProcessConnect(_connectEventArg);
            }

            return true;
        }

        public virtual bool DisconnectAsync() {
            return Disconnect();
        }

        public virtual bool ReconnectAsync() {
            if (!DisconnectAsync()) {
                return false;
            }

            while (IsConnected) {
                Thread.Yield();
            }

            return ConnectAsync();
        }

        public virtual long Send(byte[] buffer) {
            return Send(buffer.AsSpan());
        }

        public virtual long Send(byte[] buffer, long offset, long size) {
            return Send(buffer.AsSpan((int)offset, (int)size));
        }

        public virtual long Send(ReadOnlySpan<byte> buffer) {
            if (!IsHandshaked) {
                return 0L;
            }

            if (buffer.IsEmpty) {
                return 0L;
            }

            try {
                _sslStream.Write(buffer);
                long num = buffer.Length;
                BytesSent += num;
                OnSent(num, BytesPending + BytesSending);
                return num;
            }
            catch (Exception) {
                SendError(SocketError.OperationAborted);
                Disconnect();
                return 0L;
            }
        }

        public virtual long Send(string text) {
            return Send(Encoding.UTF8.GetBytes(text));
        }

        public virtual long Send(ReadOnlySpan<char> text) {
            return Send(Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public virtual bool SendAsync(byte[] buffer) {
            return SendAsync(buffer.AsSpan());
        }

        public virtual bool SendAsync(byte[] buffer, long offset, long size) {
            return SendAsync(buffer.AsSpan((int)offset, (int)size));
        }

        public virtual bool SendAsync(ReadOnlySpan<byte> buffer) {
            if (!IsHandshaked) {
                return false;
            }

            if (buffer.IsEmpty) {
                return true;
            }

            lock (_sendLock) {
                if (_sendBufferMain.Size + buffer.Length > OptionSendBufferLimit && OptionSendBufferLimit > 0) {
                    SendError(SocketError.NoBufferSpaceAvailable);
                    return false;
                }

                _sendBufferMain.Append(buffer);
                BytesPending = _sendBufferMain.Size;
                if (_sending) {
                    return true;
                }

                _sending = true;
                TrySend();
            }

            return true;
        }

        public virtual bool SendAsync(string text) {
            return SendAsync(Encoding.UTF8.GetBytes(text));
        }

        public virtual bool SendAsync(ReadOnlySpan<char> text) {
            return SendAsync(Encoding.UTF8.GetBytes(text.ToArray()));
        }

        public virtual long Receive(byte[] buffer) {
            return Receive(buffer, 0L, buffer.Length);
        }

        public virtual long Receive(byte[] buffer, long offset, long size) {
            if (!IsHandshaked) {
                return 0L;
            }

            if (size == 0L) {
                return 0L;
            }

            try {
                long num = _sslStream.Read(buffer, (int)offset, (int)size);
                if (num > 0) {
                    BytesReceived += num;
                    OnReceived(buffer, 0L, num);
                }

                return num;
            }
            catch (Exception) {
                SendError(SocketError.OperationAborted);
                Disconnect();
                return 0L;
            }
        }

        public virtual string Receive(long size) {
            byte[] array = new byte[size];
            long num = Receive(array);
            return Encoding.UTF8.GetString(array, 0, (int)num);
        }

        public virtual void ReceiveAsync() {
            TryReceive();
        }

        private void TryReceive() {
            if (_receiving || !IsHandshaked) {
                return;
            }

            try {
                while (IsHandshaked) {
                    _receiving = true;
                    IAsyncResult asyncResult = _sslStream.BeginRead(_receiveBuffer.Data, 0, (int)_receiveBuffer.Capacity, ProcessReceive, _sslStreamId);
                    if (!asyncResult.CompletedSynchronously) {
                        break;
                    }
                }
            }
            catch (ObjectDisposedException) {
            }
        }

        private void TrySend() {
            if (!IsHandshaked) {
                return;
            }

            bool flag = false;
            lock (_sendLock) {
                if (!_sendBufferFlush.IsEmpty) {
                    return;
                }

                _sendBufferFlush = Interlocked.Exchange(ref _sendBufferMain, _sendBufferFlush);
                _sendBufferFlushOffset = 0L;
                BytesPending = 0L;
                BytesSending += _sendBufferFlush.Size;
                if (_sendBufferFlush.IsEmpty) {
                    flag = true;
                    _sending = false;
                }
            }

            if (flag) {
                OnEmpty();
                return;
            }

            try {
                _sslStream.BeginWrite(_sendBufferFlush.Data, (int)_sendBufferFlushOffset, (int)(_sendBufferFlush.Size - _sendBufferFlushOffset), ProcessSend, _sslStreamId);
            }
            catch (ObjectDisposedException) {
            }
        }

        private void ClearBuffers() {
            lock (_sendLock) {
                _sendBufferMain.Clear();
                _sendBufferFlush.Clear();
                _sendBufferFlushOffset = 0L;
                BytesPending = 0L;
                BytesSending = 0L;
            }
        }

        private void OnAsyncCompleted(object sender, SocketAsyncEventArgs e) {
            if (!IsSocketDisposed) {
                if (e.LastOperation != SocketAsyncOperation.Connect) {
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }

                ProcessConnect(e);
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs e) {
            IsConnecting = false;
            if (e.SocketError == SocketError.Success) {
                if (OptionKeepAlive) {
                    Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, optionValue: true);
                }

                if (OptionTcpKeepAliveTime >= 0) {
                    Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, OptionTcpKeepAliveTime);
                }

                if (OptionTcpKeepAliveInterval >= 0) {
                    Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.BlockSource, OptionTcpKeepAliveInterval);
                }

                if (OptionTcpKeepAliveRetryCount >= 0) {
                    Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.DontRoute, OptionTcpKeepAliveRetryCount);
                }

                if (OptionNoDelay) {
                    Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, optionValue: true);
                }

                _receiveBuffer.Reserve(OptionReceiveBufferSize);
                _sendBufferMain.Reserve(OptionSendBufferSize);
                _sendBufferFlush.Reserve(OptionSendBufferSize);
                BytesPending = 0L;
                BytesSending = 0L;
                BytesSent = 0L;
                BytesReceived = 0L;
                IsConnected = true;
                OnConnected();
                try {
                    _sslStreamId = Guid.NewGuid();
                    _sslStream = ((Context.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(Socket, ownsSocket: false), leaveInnerStreamOpen: false, Context.CertificateValidationCallback) : new SslStream(new NetworkStream(Socket, ownsSocket: false), leaveInnerStreamOpen: false));
                    OnHandshaking();
                    IsHandshaking = true;
                    if (Context.Certificates != null) {
                        _sslStream.BeginAuthenticateAsClient(Address, Context.Certificates, Context.Protocols, checkCertificateRevocation: true, ProcessHandshake, _sslStreamId);
                    }
                    else if (Context.Certificate != null) {
                        _sslStream.BeginAuthenticateAsClient(Address, new X509CertificateCollection(new X509Certificate[1] { Context.Certificate }), Context.Protocols, checkCertificateRevocation: true, ProcessHandshake, _sslStreamId);
                    }
                    else {
                        _sslStream.BeginAuthenticateAsClient(Address, ProcessHandshake, _sslStreamId);
                    }

                    return;
                }
                catch (Exception) {
                    SendError(SocketError.NotConnected);
                    DisconnectAsync();
                    return;
                }
            }

            SendError(e.SocketError);
            OnDisconnected();
        }

        private void ProcessHandshake(IAsyncResult result) {
            try {
                IsHandshaking = false;
                if (IsHandshaked) {
                    return;
                }

                Guid? obj = result.AsyncState as Guid?;
                if (_sslStreamId != obj) {
                    return;
                }

                _sslStream.EndAuthenticateAsClient(result);
                IsHandshaked = true;
                TryReceive();
                if (!IsSocketDisposed) {
                    OnHandshaked();
                    if (_sendBufferMain.IsEmpty) {
                        OnEmpty();
                    }
                }
            }
            catch (Exception) {
                SendError(SocketError.NotConnected);
                DisconnectAsync();
            }
        }

        private void ProcessReceive(IAsyncResult result) {
            try {
                if (!IsHandshaked) {
                    return;
                }

                Guid? obj = result.AsyncState as Guid?;
                if (_sslStreamId != obj) {
                    return;
                }

                long num = _sslStream.EndRead(result);
                if (num > 0) {
                    BytesReceived += num;
                    OnReceived(_receiveBuffer.Data, 0L, num);
                    if (_receiveBuffer.Capacity == num) {
                        if (2 * num > OptionReceiveBufferLimit && OptionReceiveBufferLimit > 0) {
                            SendError(SocketError.NoBufferSpaceAvailable);
                            DisconnectAsync();
                            return;
                        }

                        _receiveBuffer.Reserve(2 * num);
                    }
                }

                _receiving = false;
                if (num > 0) {
                    if (!result.CompletedSynchronously) {
                        TryReceive();
                    }
                }
                else {
                    DisconnectAsync();
                }
            }
            catch (Exception) {
                SendError(SocketError.OperationAborted);
                DisconnectAsync();
            }
        }

        private void ProcessSend(IAsyncResult result) {
            try {
                if (!IsHandshaked) {
                    return;
                }

                Guid? obj = result.AsyncState as Guid?;
                if (_sslStreamId != obj) {
                    return;
                }

                _sslStream.EndWrite(result);
                long size = _sendBufferFlush.Size;
                if (size > 0) {
                    BytesSending -= size;
                    BytesSent += size;
                    _sendBufferFlushOffset += size;
                    if (_sendBufferFlushOffset == _sendBufferFlush.Size) {
                        _sendBufferFlush.Clear();
                        _sendBufferFlushOffset = 0L;
                    }

                    OnSent(size, BytesPending + BytesSending);
                }

                TrySend();
            }
            catch (Exception) {
                SendError(SocketError.OperationAborted);
                DisconnectAsync();
            }
        }

        protected virtual void OnConnecting() {
        }

        protected virtual void OnConnected() {
        }

        protected virtual void OnHandshaking() {
        }

        protected virtual void OnHandshaked() {
        }

        protected virtual void OnDisconnecting() {
        }

        protected virtual void OnDisconnected() {
        }

        protected virtual void OnReceived(byte[] buffer, long offset, long size) {
        }

        protected virtual void OnSent(long sent, long pending) {
        }

        protected virtual void OnEmpty() {
        }

        protected virtual void OnError(SocketError error) {
        }

        private void SendError(SocketError error) {
            if (error != SocketError.ConnectionAborted && error != SocketError.ConnectionRefused && error != SocketError.ConnectionReset && error != SocketError.OperationAborted && error != SocketError.Shutdown) {
                OnError(error);
            }
        }

        public void Dispose() {
            Dispose(disposingManagedResources: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposingManagedResources) {
            if (!IsDisposed) {
                if (disposingManagedResources) {
                    DisconnectAsync();
                }

                IsDisposed = true;
            }
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