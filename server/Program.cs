using MessageHandler;
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
        public const string IP = "127.0.0.1";
        public const int PORT = 8080;
        public static TcpListener server;
        static void Main(string[] args)
        {
            int N = 0;
            bool res = false;

            do
            {
                Console.Clear();
                Console.WriteLine("Максимальное количество одновременно обрабатываемых запросов должно быть целым числом и больше 0");
                Console.Write("N: ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out N) && N > 0)
                    res = true;
            } while (!res);

            SemaphoreSlim semSlim = new SemaphoreSlim(N, N);

            try
            {
                server = new TcpListener(IPAddress.Parse(IP), PORT);
                server.Start();
                Console.WriteLine("\nФормат выводимых данных: [Время получения запроса] [IP адрес клиента] [Индекс записи в таблице] [Сообщение] [Статус]");
                Console.WriteLine("Ожидание подключений...\n");
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    Thread thread = new Thread(() =>
                    {
                        if (semSlim.CurrentCount > 0)
                        {
                            semSlim.Wait();
                            try
                            {
                                StartMessageHandler(stream, client);
                                Thread.Sleep(5000);
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

                            semSlim.Release();
                        }
                        else
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt")} {client.Client.RemoteEndPoint} [-] [-] [Запрос не был обработан. Количество запросов превышено!]");
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
                server.Stop();
            }
        }
        public static void StartMessageHandler(NetworkStream stream, TcpClient client)
        {
            Request receivedRequest = MessageHandler.MessageHandler.ReadMessage(stream);
            string answer = IsPalindrome(receivedRequest.Message);
            receivedRequest.Status = answer;
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt")} {client.Client.RemoteEndPoint} [{receivedRequest.Id + 1}] [{receivedRequest.Message}] [{answer}]");
            MessageHandler.MessageHandler.SendMessage(stream, receivedRequest);
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
    }
}
