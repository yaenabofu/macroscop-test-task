using MessageHandler;
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
        private const string IP = "127.0.0.1";
        private const int PORT = 8080;
        private static TcpListener server;
        private static void Main(string[] args)
        {
            int MaxNumberOfRequests = 0;
            bool IsValidValue = false;

            do
            {
                Console.Clear();
                Console.WriteLine("Значение должно быть целым числом и больше 0");
                Console.Write("Максимальное количество одновременно обрабатываемых запросов: ");
                string inputValue = Console.ReadLine();
                if (int.TryParse(inputValue, out MaxNumberOfRequests) && MaxNumberOfRequests > 0)
                    IsValidValue = true;
            } while (!IsValidValue);

            SemaphoreSlim semSlim = new SemaphoreSlim(MaxNumberOfRequests, MaxNumberOfRequests);

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
        private static void StartMessageHandler(NetworkStream stream, TcpClient client)
        {
            Request receivedRequest = MessageHandler.MessageHandler.ReadMessage(stream);
            string answer = IsPalindrome(receivedRequest.Message);
            receivedRequest.Status = answer;
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt")} {client.Client.RemoteEndPoint} [{receivedRequest.Id + 1}] [{receivedRequest.Message}] [{answer}]");
            MessageHandler.MessageHandler.SendMessage(stream, receivedRequest);
        }
        private static string IsPalindrome(string inputString)
        {
            char[] charSeparators = new char[] {' ', ',', '.', '"', '<', '>', '/', ':', ';', '{', '}', '[', ']', '-', '=', '+', '_', '|', '*', '%',
                '!', '@', '#', '№', '^', '&', '?', '(', ')'};
            string[] result = inputString.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            string afterFormating = string.Join("", result).ToLower();

            if (afterFormating.Length < 2)
            {
                return "Не палиндром";
            }

            for (int i = 0; i < afterFormating.Length / 2; i++)
            {
                if (afterFormating[i] != afterFormating[afterFormating.Length - i - 1])
                    return "Не палиндром";
            }

            return "Палиндром";
        }
    }
}
