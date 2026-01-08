using NetCoreServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ChatClient : WssClient {
    public ChatClient(SslContext context, IPAddress address, int port) : base(context, address, port) { }

    public void DisconnectAndStop() {
        _stop = true;
        CloseAsync(1000);
        while (IsConnected)
            Thread.Yield();
    }

    /*public override void OnWsConnecting(HttpRequest request) {
        request.SetBegin("GET", "/");
        request.SetHeader("Host", "localhost");
        request.SetHeader("Origin", "http://localhost");
        request.SetHeader("Upgrade", "websocket");
        request.SetHeader("Connection", "Upgrade");
        request.SetHeader("Sec-WebSocket-Key", Convert.ToBase64String(WsNonce));
        request.SetHeader("Sec-WebSocket-Protocol", "chat, superchat");
        request.SetHeader("Sec-WebSocket-Version", "13");
        request.SetBody();
    }*/

    public override void OnWsConnected(HttpResponse response) {
        Debug.Log($"Chat WebSocket client connected a new session with Id {Id}");
    }

    public override void OnWsDisconnected() {
        Debug.Log($"Chat WebSocket client disconnected a session with Id {Id}");
    }

    public override void OnWsReceived(byte[] buffer, long offset, long size) {
        Debug.Log($"Incoming: {Encoding.UTF8.GetString(buffer, (int)offset, (int)size)}");
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();

        Debug.Log($"Chat WebSocket client disconnected a session with Id {Id}");

        // Wait for a while...
        Thread.Sleep(1000);

    }

    protected override void OnError(SocketError error) {
        Debug.Log($"Chat WebSocket client caught an error with code {error}");
    }

    private bool _stop;
}