using NetCoreServer;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;


class Program
{
    static void Main(string[] args)
    {
        int portTcp = 7050;
        int portWs = 7051;
        if (args.Length > 0) {
            portTcp = int.Parse(args[0]);
        }
        if (args.Length > 1) {
            portWs = int.Parse(args[1]);
        }

        string www = "./";

        Console.WriteLine($"TCP server port: {portTcp}");
        Console.WriteLine($"Websocket server port: {portWs}");

        var server = new Server();

        IPAddress ip = IPAddress.Any;
        Console.WriteLine(ip);
        var tcp = new PiratesServerTcp(ip, portTcp, server);

        var cert = X509Certificate2.CreateFromPemFile("domain.crtca", "domain.key");
        
        var context = new SslContext(SslProtocols.Tls & SslProtocols.Ssl3, cert);
        
        var ws = new PiratesServerWs(ip, portWs, server, context);
        ws.AddStaticContent(www, "/chat");

        Console.Write("Server starting...");
        tcp.Start();
        ws.Start();
        SaveLoadSystem.Initialize();
        Console.WriteLine("Server started!");

        Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

        for (; ; )
        {
            string line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            if (line == "!")
            {
                Console.Write("Server restarting...");
                tcp.Restart();
                ws.Restart();
                Console.WriteLine("Server restarted!");
                continue;
            }
        }
        
        Console.Write("Server stopping...");
        tcp.Stop();
        ws.Stop();
        SaveLoadSystem.CloseConnection();
        Console.WriteLine("Server stopped!");
    }
}