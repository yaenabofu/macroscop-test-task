using client;
using Newtonsoft.Json;
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
            //int N = 0;
            //do
            //{
            //    Console.Write("N: ");
            //} while (!int.TryParse(Console.ReadLine(), out N));

            //Semaphore sem = new Semaphore(N, N);

            try
            {
                server = new TcpListener(IPAddress.Parse(IP), PORT);
                server.Start();
                Console.WriteLine("Waiting for connections...\n");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    Thread thread = new Thread(() =>
                    {
                        try
                        {
                            StartMessageHandler(stream, client);
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            if (stream != null)
                                stream.Close();
                            if (client != null)
                                client.Close();
                        }
                    });
                    thread.Start();
                }
            }
            catch (Exception)
            {

            }
        }
        public static void StartMessageHandler(NetworkStream stream, TcpClient client)
        {
            Request receivedRequest = ReadMessage(stream, client);

            if (receivedRequest != null)
            {
                string answer = IsPalindrome(receivedRequest.Message);
                Thread.Sleep(1000);
                receivedRequest.Status = answer;
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt")} {client.Client.RemoteEndPoint} [{receivedRequest.Id}][{receivedRequest.Message}][{answer}]");
                SendMessage(stream, receivedRequest, client);
            }
        }
        public static string IsPalindrome(string str)
        {
            if (str.Length < 2)
            {
                return "Не палиндром";
            }

            for (int i = 0; i < str.Length / 2; i++)
            {
                if (str[i] != str[str.Length - i - 1])
                    return "Не палиндром";
            }

            return "Палиндром";
        }
        public static Request ReadMessage(NetworkStream stream, TcpClient client)
        {
            StringBuilder completeMessage = new StringBuilder();

            int bytes = 0;
            byte[] data = new byte[256];

            do
            {
                bytes = stream.Read(data, 0, data.Length);
                completeMessage.AppendFormat("{0}", Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            return JsonConvert.DeserializeObject<Request>(completeMessage.ToString());
        }
        public static void SendMessage(NetworkStream stream, Request request, TcpClient client)
        {
            byte[] data = new byte[256];
            string json = JsonConvert.SerializeObject(request);
            data = Encoding.UTF8.GetBytes(json);

            if (data.Length == 0)
                return;

            stream.Write(data, 0, data.Length);
        }
    }
}
