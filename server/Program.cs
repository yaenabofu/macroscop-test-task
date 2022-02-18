using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace server
{
    internal class Program
    {
        public static TcpListener server;
        public const string IP = "127.0.0.1";
        public const int PORT = 8080;
        static void Main(string[] args)
        {
            Console.Write("N: ");
            int N = int.Parse(Console.ReadLine());

            try
            {
                server = new TcpListener(IPAddress.Parse(IP), PORT);
                server.Start();
                Console.WriteLine("\nWaiting for connections...\n");

                while (true)
                {
                    Thread.Sleep(100);
                    TcpClient client = server.AcceptTcpClient();
                    try
                    {
                        Task task = new Task(() => { StartMessageHandler(client); });
                        task.Start();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static void StartMessageHandler(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            if (acceptedUsers.Where(x => x.Nickname == user.Nickname).Count() == 0)
            {
                SendMessage("true", user);
                acceptedUsers.Add(user);
                SendMessageToEveryone($"[{DateTime.Now.ToShortTimeString()}] {user.Nickname} connected" + "\n");
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] {user.Nickname} connected" + "\n");

                string incomingMessage = null;
                while (user.Client.Connected)
                {
                    try
                    {
                        incomingMessage = ReadMessage(user) + "\n";
                        if (incomingMessage != "\n")
                            Console.WriteLine(incomingMessage);
                        SendMessageToEveryone(incomingMessage);
                    }
                    catch (Exception)
                    {
                        stream.Close();
                        client.Close();
                        acceptedUsers.Remove(user);
                        Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] (*_*) {user.Nickname} disconnected (*_*)" + "\n");
                    }
                }
            }
            else
            {
                SendMessage("false", user);
                stream.Close();
                client.Close();
            }

        }
        public static string ReadMessage(User user)
        {
            int bytes = 0;
            byte[] data = new byte[256];
            StringBuilder completeMessage = new StringBuilder();

            do
            {
                bytes = user.Stream.Read(data, 0, data.Length);
                completeMessage.AppendFormat("{0}", Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (user.Stream.DataAvailable);

            return completeMessage.ToString();
        }
        public static void SendMessage(string message, User user)
        {
            byte[] data = new byte[256];
            data = Encoding.UTF8.GetBytes(message);
            user.Stream.Write(data, 0, data.Length);
        }
    }
}
