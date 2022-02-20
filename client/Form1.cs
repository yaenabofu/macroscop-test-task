using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
            btnAddFile.Enabled = false;
        }
        public const int PORT = 8080;
        public const string IP = "127.0.0.1";
        public TcpClient client = new TcpClient();
        public NetworkStream stream = null;
        public int id = 0;

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

            dataGridView1.Rows.Add(id + 1, fileContent, "Не отправлено");
            dataGridView1.Rows[id].Tag = id;
            id++;
        }

        public void StartMessageManager(Request request)
        {
            Request incomingRequest = new Request();
            Thread thread = new Thread(() =>
           {
               try
               {
                   Thread.Sleep(1000);
                   SendMessage(stream, request);
                   incomingRequest = ReadMessage(stream);
                   Invoke((Action)(() =>
                   {
                       dataGridView1.Rows[incomingRequest.Id].Cells[2].Value = incomingRequest.Status;
                   }));
               }
               catch (Exception)
               {
                   try
                   {
                       Invoke((Action)(() =>
                                         {
                                             btnAddFile.Enabled = false;
                                             btnSendFiles.Enabled = false;
                                             btnConnect.Text = "Подключиться к серверу";
                                         }));
                   }
                   catch (Exception)
                   {

                   }


                   stream.Close();
                   client.Close();
                   MessageBox.Show("Сервер разорвал соединение");
               }
           });
            thread.Start();
        }
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

        private void btnSendFiles_Click(object sender, EventArgs e)
        {
            btnSendFiles.Enabled = false;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (dataGridView1.Rows[i].Cells[2].Value.ToString() == "Не отправлено")
                {
                    int dgw_id = int.Parse(dataGridView1.Rows[i].Cells[0].Value.ToString()) - 1;
                    string dgw_message = dataGridView1.Rows[i].Cells[1].Value.ToString();
                    try
                    {
                        StartMessageManager(new Request(dgw_id, "", dgw_message));
                    }
                    catch (Exception)
                    {
                        Invoke((Action)(() =>
                        {
                            dataGridView1.Rows[dgw_id].Cells[2].Value = "Ошибка при обработке запроса";
                        }));
                    }
                }
            }
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
                this.btnSendFiles.Enabled = true;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!client.Connected)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(IP, PORT);
                    stream = client.GetStream();
                    btnAddFile.Enabled = true;
                    btnConnect.Text = "Отключиться от сервера";
                    MessageBox.Show("Успешное подключение к серверу");
                }
                catch (Exception)
                {
                    MessageBox.Show("Не удалось подключиться к серверу");
                }
            }
            else
            {
                btnAddFile.Enabled = false;
                btnSendFiles.Enabled = false;
                btnConnect.Text = "Подключиться к серверу";
                stream.Close();
                client.Close();
            }
        }
    }
}

