using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using TcpClient = NetCoreServer.TcpClient;

//using WebSocketSharp;
using NativeWebSocket;

#if UNITY_WEBGL
public class Client
#else
public class Client : TcpClient
#endif
{
    public UnityClient client;

#if UNITY_WEBGL
    private WebSocket _connection;

    private string _addr;
    private int _port;
#endif


    public Client(string address, int port)
#if !UNITY_WEBGL
        : base(address, port)
#endif
    {
#if UNITY_WEBGL
        _addr = address;
        _port = port;
        //_connection = new WebSocket("wss://ws.postman-echo.com/raw");
        _connection = new WebSocket($"wss://12bitgame.ru:{port}");
        //_connection = new WebSocket($"wss://{_addr}:{port}", "wss");

        //Debug.Log($"wss://{_addr}:{port}");

        _connection.OnError += (e) => {
            OnError(e);
        };

        _connection.OnClose += (e) => {
            OnDisconnected();
        };
        _connection.OnMessage += (bytes) => {
            OnReceived(Encoding.UTF8.GetString(bytes));
        };
        _connection.OnOpen += () => { OnConnected(); };

#endif
    }

    public async void Conn() {
#if UNITY_WEBGL
        await _connection.Connect();
#else
        ConnectAsync();
#endif
    }

    public void DispatchMessages() {
#if UNITY_WEBGL && UNITY_EDITOR
        _connection.DispatchMessageQueue();
#endif
    }

    public async void SendDataAsync(string data) {
#if UNITY_WEBGL
        await _connection.SendText(data);
#else
        SendAsync(data);
#endif
    }

    public void DisconnectAndStop() {
        _stop = true;
#if UNITY_WEBGL
        _connection.Close();
#else
        DisconnectAsync();
        while (IsConnected)
            Thread.Yield();
#endif
    }

#if UNITY_WEBGL
    protected void OnConnected()
#else
    protected override void OnConnected()
#endif
    {
        Debug.Log($"Connected!");
        ResponceBase data = new ResponceBase();
        client.passedJson.Add(data, "");
        client.actions.Add(data, client.OnConnected);
    }

#if UNITY_WEBGL
    protected void OnDisconnected()
#else
    protected override void OnDisconnected() 
#endif
    {
        ResponceBase response = new() { Result = ResponseResult.Error };
        client.actions.Add(response, (UnityEvent<string>)client.Commands[response.Result]);
        client.passedJson.Add(response, "");
    }

#if UNITY_WEBGL
    public void OnReceived(string message)
#else
    protected override void OnReceived(byte[] buffer, long offset, long size) 
#endif
    {
#if UNITY_WEBGL
        string json = message;
        //Debug.Log(json);
#else
        string json = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
#endif
        string[] jsons = json.Split("}{", StringSplitOptions.None);
        try {
            foreach (string js in jsons) {
                string js1 = js;
                if (!js1.StartsWith('{'))
                    js1 = "{" + js1;
                if (!js1.EndsWith('}'))
                    js1 = js1 + "}";

                ResponceBase response = JsonUtility.FromJson<ResponceBase>(js1);
                client.actions.Add(response, (UnityEvent<string>)client.Commands[response.Result]);
                client.passedJson.Add(response, js1);
            }
        }
        catch (Exception e) {
            Debug.Log(e);
        }
    }

#if UNITY_WEBGL
    protected void OnError(string error)
#else
    protected override void OnError(SocketError error)
#endif
    {
        Debug.Log($"Error {error}");
    }

    private bool _stop;
}