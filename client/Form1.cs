using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            btnSendFiles.Enabled = false;
        }
        public const int PORT = 8080;
        public const string IP = "127.0.0.1";
        public static TcpClient client = new TcpClient();
        public static NetworkStream stream = null;
        public int id = 0;
        public Dictionary<int, Request> requests = new Dictionary<int, Request>();

        private void btnAddFile_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }

            requests.Add(id, new Request(id, "Не отправлено", fileContent));

            dataGridView1.Rows.Add(id + 1, fileContent, "Не отправлено");
            dataGridView1.Rows[id].Tag = id;
            id++;
        }

        public async void StartMessageManager(Dictionary<int, Request> requests, NetworkStream stream)
        {
            try
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    await Task.Run(() =>
                    {
                        client = new TcpClient();
                        client.Connect(IP, PORT);
                        stream = client.GetStream();
                        int id = requests[i].Id;

                        try
                        {
                            SendMessage(stream, requests[i].Message);
                            string receivedMessage = ReadMessage();

                            Invoke((Action)(() =>
                                                {
                                                    dataGridView1.Rows[id].Cells[2].Value = receivedMessage;
                                                }));
                        }
                        catch (Exception)
                        {
                            Invoke((Action)(() =>
                            {
                                dataGridView1.Rows[id].Cells[2].Value = "Ошибка при обработке запроса";
                            }));
                        }

                        stream.Close();
                        client.Close();
                        requests.Remove(id);
                    });
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка в соединении с сервером");
            }
        }
        public static string ReadMessage()
        {
            int bytes = 0;
            byte[] data = new byte[256];
            StringBuilder completeMessage = new StringBuilder();

            do
            {
                bytes = stream.Read(data, 0, data.Length);
                completeMessage.AppendFormat("{0}", Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            return completeMessage.ToString();
        }
        public static void SendMessage(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        private void btnSendFiles_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[2].Value = "Ожидание ответа";
            }

            StartMessageManager(requests, stream);
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
                this.btnSendFiles.Enabled = true;
        }
    }
}

