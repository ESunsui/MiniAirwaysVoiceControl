using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpeechRecognitionApp
{
    public class TCPClient
    {
        private const int BasePort = 9000;

        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private StreamReader reader;
        private StreamWriter writer;
        private Task receiveTask;

        public bool IsConnecting
        {
            get { return tcpClient != null && !tcpClient.Connected; }
        }

        public bool IsConnected
        {
            get { return tcpClient != null && tcpClient.Connected && reader != null; }
        }

        public delegate void OnMessageReceivedHandler(string message);
        public event OnMessageReceivedHandler OnMessageReceived;

        /// <summary>
        /// 连接到服务器。pipeRnd 可以用来指定端口偏移量以支持多实例连接。
        /// </summary>
        /// <param name="portRnd">端口的随机偏移量（可选）</param>
        /// <returns>连接是否成功</returns>
        public async Task<bool> Connect(string portRnd = "")
        {
            if (IsConnecting || IsConnected)
            {
                return true;
            }

            Console.WriteLine("Connecting to Mini Airways...");

            try
            {
                tcpClient = new TcpClient();

                // 计算端口号
                int port = BasePort;
                if (!string.IsNullOrEmpty(portRnd) && int.TryParse(portRnd, out int rndPort))
                {
                    port = rndPort;
                }

                await tcpClient.ConnectAsync(IPAddress.Loopback, port);
                Console.WriteLine("Successfully connected to Mini Airways.");

                networkStream = tcpClient.GetStream();
                reader = new StreamReader(networkStream, Encoding.UTF8);
                writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };

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

        /// <summary>
        /// 启动接收消息的任务
        /// </summary>
        private void StartReceive()
        {
            receiveTask = Task.Run(async () =>
            {
                while (reader != null)
                {
                    try
                    {
                        string message = await reader.ReadLineAsync();
                        if (message == null)
                        {
                            Console.WriteLine("Mini Airways disconnected.");
                            Close();
                            break;
                        }

                        Console.WriteLine(message);
                        OnMessageReceived?.Invoke(message);
                    }
                    catch
                    {
                        Console.WriteLine("Error receiving data.");
                        Close();
                    }
                }
            });
        }

        /// <summary>
        /// 发送消息到服务器
        /// </summary>
        /// <param name="message">要发送的消息</param>
        public async Task Send(string message)
        {
            if (!IsConnected)
            {
                Console.WriteLine("Not connected to Mini Airways.");
                return;
            }

            try
            {
                await writer.WriteLineAsync(message);
                // writer.Flush(); // 已经设置了 AutoFlush 为 true
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send message to Mini Airways.");
                Console.WriteLine(ex.Message);
                Close();
            }
        }

        /// <summary>
        /// 关闭连接并释放资源
        /// </summary>
        public void Close()
        {
            // 取消接收任务
            if (receiveTask != null)
            {
                // 如果需要，可以使用取消令牌来优雅地取消任务
                receiveTask.Dispose();
                receiveTask = null;
            }

            // 关闭读取器
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
                reader = null;
            }

            // 关闭写入器
            if (writer != null)
            {
                writer.Close();
                writer.Dispose();
                writer = null;
            }

            // 关闭网络流
            if (networkStream != null)
            {
                networkStream.Close();
                networkStream.Dispose();
                networkStream = null;
            }

            // 关闭 TCP 客户端
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient.Dispose();
                tcpClient = null;
            }

            Console.WriteLine("Connection closed.");
        }
    }
}