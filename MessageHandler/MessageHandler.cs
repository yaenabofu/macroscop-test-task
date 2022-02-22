using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageHandler
{
    public static class MessageHandler
    {
        public static Request ReadMessage(NetworkStream stream)
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
        public static void SendMessage(NetworkStream stream, Request request)
        {
            byte[] data = new byte[256];
            string json = JsonConvert.SerializeObject(request);
            data = Encoding.UTF8.GetBytes(json);
            stream.Write(data, 0, data.Length);
        }
    }
}
