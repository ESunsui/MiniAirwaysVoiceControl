using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechRecognitionApp
{
    public class NamedPipeClient
    {
        const string pipeName = "MiniAirwaysVOControl";
        NamedPipeClientStream pipeClient;
        StreamReader reader;
        StreamWriter writer;
        Task receiveTask;

        public bool IsConnecting { get { return pipeClient != null; } }
        public bool IsConnected { get { return reader != null; } }

        public delegate void OnMessageReceivedHandler(string message);
        public event OnMessageReceivedHandler OnMessageReceived;

        public async Task<bool> Connect()
        {
            if (IsConnecting || IsConnected)
            {
                return true;
            }
            Console.WriteLine("Connecting to Mini Airways...");
            try
            {
                pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await pipeClient.ConnectAsync();
                Console.WriteLine("Successfully connected to Mini Airways.");
                reader = new StreamReader(pipeClient);
                writer = new StreamWriter(pipeClient);
                StartReceive();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to connect to Mini Airways.");
                Console.WriteLine(e.Message);
                Close();
                return false;
            }
        }

        void StartReceive()
        {
            receiveTask = Task.Run(async () =>
            {
                while (reader != null)
                {
                    try
                    {
                        string message = await reader.ReadLineAsync();
                        Console.WriteLine(message);
                        OnMessageReceived.Invoke(message);
                    }
                    catch 
                    {
                        Close();
                    }
                }
            });
        }


        public async Task Send(string message)
        {
            if (!pipeClient.IsConnected)
            {
                Console.WriteLine("Not connected to Mini Airways.");
                return;
            }
            try
            {
                await writer.WriteLineAsync(message);
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send message to Mini Airways.");
                Console.WriteLine(ex.Message);
                Close();
            }
        }

        public void Close()
        {
            
            if (receiveTask != null)
            {
                receiveTask.Dispose();
                receiveTask = null;
            }
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
                reader = null;
            }
            if (writer != null)
            {
                writer.Close();
                writer.Dispose();
                writer = null;
            }
            if (pipeClient != null)
            {
                pipeClient.Close();
                pipeClient.Dispose();
                pipeClient = null;
            }
        }
    }
}
