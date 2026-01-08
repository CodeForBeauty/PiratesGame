using NetCoreServer;
using System.Collections;
using System.Net.Sockets;
using System.Net;

using WebSocketSharp.Server;
using WebSocketSharp;

public class PiratesServerTcp : TcpServer {
    public Server BaseServer;

    public PiratesServerTcp(IPAddress address, int port, Server server) : base(address, port) {
        BaseServer = server;
    }

    protected override TcpSession CreateSession() {
        PlayerSessTcp sess = new(this, BaseServer);
        return sess;
    }

    protected override void OnError(SocketError error) {
        Console.WriteLine($"[ERROR] {(int)error}");
    }
}

/*public class PiratesServerWs : WebSocketBehavior {
    public Server BaseServer;

    *//*public PiratesServerWs(IPAddress address, int port, Server server) : base() {
        BaseServer = server;
    }*//*

    protected override void OnMessage(MessageEventArgs e) {
        Console.WriteLine(e.Data);
        Send("asd");
    }

    *//*protected override WsSession CreateSession() {
        PlayerSessWs sess = new(this, BaseServer);
        return sess;
    }

    protected override void OnError(SocketError error) {
        Console.WriteLine($"[ERROR] {(int)error}");
    }*//*
}*/

public class PiratesServerWs : WssServer {
    public Server BaseServer;
    
    public PiratesServerWs(IPAddress address, int port, Server server, SslContext context) : base(context, address, port) {
        BaseServer = server;
    }

    protected override SslSession CreateSession() {
        PlayerSessWs sess = new(this, BaseServer);
        return sess;
    }

    protected override void OnError(SocketError error) {
        Console.WriteLine($"[ERROR] {(int)error}");
    }
}