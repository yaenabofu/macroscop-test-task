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
        private string status;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Status
        {
            get { return status; }
            set { status = value; }
        }
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        public Request()
        {
            id = 0;
            status = string.Empty;
            message = string.Empty;
        }
        public Request(int id, string status, string message)
        {
            Id = id;
            Status = status;
            Message = message;
        }
    }
}
