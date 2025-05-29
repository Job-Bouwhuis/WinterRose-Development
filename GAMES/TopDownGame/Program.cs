using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Net.NetworkInformation;
using System.Diagnostics;



//WinterRose.Windows.OpenConsole();

//Thread serverThread = new Thread(Server);
//serverThread.Start();

//Thread.Sleep(500);

//Client();

//Console.WriteLine("\n\n\ndone");
//Console.ReadLine();
//return;
//static void Server()
//{
//    TcpListener serverListener = new TcpListener(IPAddress.Loopback, 12345);
//    serverListener.Start();
//    Console.WriteLine("Server started... Waiting for a connection...");
//    TcpClient client = serverListener.AcceptTcpClient();
//    Console.WriteLine("Client connected!");
//    NetworkStream networkStream = client.GetStream();
//    StreamReader reader = new StreamReader(networkStream);
//    while (true)
//    {
//        WinterForge.SerializeToStream(new Vector3(1, 2, 3), networkStream);
        
//        string ack = reader.ReadLine();

//        if (ack == "OK")
//        {
//            Console.WriteLine("sending next data...");
//        }
//        else
//        {
//            Console.WriteLine("Error: Expected OK from client.");
//            break;
//        }
//    }
//}

//static void Client()
//{
//    TcpClient client = new TcpClient("127.0.0.1", 12345);
//    Console.WriteLine("Client connected to server...");
//    NetworkStream networkStream = client.GetStream();
//    StreamWriter writer = new StreamWriter(networkStream) { AutoFlush = true };

//    int i = 0;
//    int max = 20;
//    while (i++ < max)
//    {
//        Stopwatch sw = Stopwatch.StartNew();
//        Vector3 vec = WinterForge.DeserializeFromStream<Vector3>(networkStream);
//        sw.Stop();
//        Console.WriteLine("Received: " + vec.ToString());
//        Console.WriteLine("Deserialization Time: " + sw.ElapsedMilliseconds + "ms" + 
//            $"\t{sw.Elapsed.TotalMicroseconds}micro");

//        writer.WriteLine("OK");
//    }
//}


using var game = new TopDownGame.Game1();
game.Run();


