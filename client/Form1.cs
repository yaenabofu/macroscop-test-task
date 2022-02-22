using MessageHandler;
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

        public void StartMessageManager(Request request, NetworkStream stream, TcpClient client)
        {
            Request incomingRequest = request;
            Thread thread = new Thread(() =>
           {
               try
               {
                   btnSendFiles.Enabled = false;
                   btnAddFile.Enabled = true;
                   client = new TcpClient();
                   client.Connect(IP, PORT);
                   stream = client.GetStream();
                   MessageHandler.MessageHandler.SendMessage(stream, request);
                   incomingRequest = MessageHandler.MessageHandler.ReadMessage(stream);
                   Invoke((Action)(() =>
                   {
                       dataGridView1.Rows[incomingRequest.Id].Cells[2].Value = incomingRequest.Status;
                   }));
               }
               catch (Exception)
               {

                   Invoke((Action)(() =>
                   {
                       dataGridView1.Rows[incomingRequest.Id].Cells[2].Value = "Ошибка при обработке запроса";
                   }));
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

        private void btnSendFiles_Click(object sender, EventArgs e)
        {
            btnSendFiles.Enabled = false;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (dataGridView1.Rows[i].Cells[2].Value.ToString() == "Не отправлено")
                {
                    int dgw_id = int.Parse(dataGridView1.Rows[i].Cells[0].Value.ToString()) - 1;
                    string dgw_message = dataGridView1.Rows[i].Cells[1].Value.ToString();
                    StartMessageManager(new Request(dgw_id, "", dgw_message), stream, client);
                }
            }
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
                this.btnSendFiles.Enabled = true;
        }
    }
}

