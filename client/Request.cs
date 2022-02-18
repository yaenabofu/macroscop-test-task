using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    public class Request
    {
        private int id;
        private string message;
        private string answer;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Answer
        {
            get { return answer; }
            set { answer = value; }
        }
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        public Request()
        {
            id = 0;
            answer = string.Empty;
            message = string.Empty;
        }
        public Request(int id, string asnwer, string message)
        {
            Id = id;
            Answer = asnwer;
            Message = message;
        }
    }
}
