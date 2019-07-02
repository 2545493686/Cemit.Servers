using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Cemit.Servers
{
    // 异步UDP类
    public class UdpServer : IServer
    {
        /// <summary>
        /// 接收器类型和数据的分隔符
        /// </summary>
        const char c_Split = '\0';

        // 定义UDP发送和接收
        readonly UdpClient m_UdpClient;
        readonly IPEndPoint m_LocalIP;

        ILogger m_Logger;

        struct ReceiveState
        {
            public UdpClient client;
            public IReceiversFactory receiversFactory;
        }

        public UdpServer()
        {
            m_UdpClient = new UdpClient();
            m_LocalIP = (IPEndPoint)m_UdpClient.Client.LocalEndPoint;
        }

        public UdpServer(ILogger logger) : this()
        {
            m_Logger = logger;
        }

        public UdpServer(IPEndPoint localIP)
        {
            m_LocalIP = localIP;
            m_UdpClient = new UdpClient(localIP);
        }

        public UdpServer(IPEndPoint localIP, ILogger logger) : this(localIP)
        {
            m_Logger = logger;
        }

        public IServer Run(IReceiversFactory receiversFactor)
        {
            new Thread(() => Receive(receiversFactor)).Start();
            return this;
        }

        private void Receive(IReceiversFactory receiversFactor)
        {
            Log($"开始监听！端口 {m_LocalIP.Port}");
            m_UdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), new ReceiveState
            {
                client = m_UdpClient,
                receiversFactory = receiversFactor
            });
        }

        // 接收回调函数
        private void ReceiveCallback(IAsyncResult iar)
        {
            IPEndPoint ip = null;
            ReceiveState state = (ReceiveState)iar.AsyncState;
            byte[] receiveBytes = state.client.EndReceive(iar, ref ip);
            m_UdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), state);

            if (iar.IsCompleted)
            {
                new Thread(() =>
                {
                    string receiveContent = Encoding.ASCII.GetString(receiveBytes);
                    Log($"Received: {receiveContent} [{ip.ToString()}]");

                    if (string.IsNullOrEmpty(receiveContent))
                    {
                        Log($"Received ERROR: null content [{ip.ToString()}]");
                        return;
                    }
                    else if (!receiveContent.Contains(c_Split.ToString()))
                    {
                        Log($"Received ERROR: can not find [{c_Split}] in [{receiveContent}] [{ip.ToString()}]");
                        return;
                    }

                    ReceiveContent content = new ReceiveContent(receiveContent, c_Split);

                    IReceiver receiver = state.receiversFactory.GetReceiver(content.receiverType);

                    if (receiver == null)
                    {
                        Log($"Received ERROR: type error! [{content.receiverType}] [{ip.ToString()}]");
                        return;
                    }

                    string receipt = receiver.Receive(ip, content.content);

                    if (receipt != null)
                    {
                        Send(ip, content.receiverType, receipt);
                    }

                }).Start();
            }
        }

        // 发送函数
        public IServer Send(IPEndPoint IP, string receiverType, string content = null)
        {
            byte[] sendBytes = Encoding.ASCII.GetBytes(receiverType + c_Split + content);
            m_UdpClient.Send(sendBytes, sendBytes.Length, IP);
            Log($"已向 [{IP.ToString()}] 发送：{receiverType + c_Split + content}");
            return this;
        }

        private void Log(string content)
        {
            if (m_Logger == null)
                return;

            m_Logger.Log(content);
        }

        struct ReceiveContent
        {
            public string receiverType;
            public string content;

            public ReceiveContent(string receiveContent, char splitText)
            {
                receiverType = receiveContent.Split(splitText)[0];
                content = receiveContent.Split(splitText)[1];
            }
        }
    }
}
