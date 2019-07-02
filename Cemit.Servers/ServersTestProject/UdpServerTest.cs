using Cemit.Servers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ServersTestProject
{
    [TestClass]
    public class UdpServerTest
    {
        class TestDataReceiver : IReceiver
        {
            public string Content { get; private set; }

            public string Receive(IPEndPoint ip, string content)
            {
                Content = content;
                return null;
            }
        }

        class TestNullReceiver : IReceiver
        {
            public bool HasReceived { get; set; } = false;
            public string Receive(IPEndPoint ip, string content)
            {
                HasReceived = true;
                return null;
            }
        }

        class TestReceiversFactory : IReceiversFactory
        {
            readonly IReceiver m_Receiver;

            public TestReceiversFactory(IReceiver receiver)
            {
                m_Receiver = receiver;
            }

            public IReceiver GetReceiver(string type)
            {
                return m_Receiver;
            }
        }

        class TestLog : ILogger
        {
            readonly string m_Name;

            public TestLog(string name)
            {
                m_Name = name;
            }

            public void Log(string content)
            {
                Console.WriteLine($"{m_Name}: {content}");
            }
        }

        [TestMethod]
        public void TestNullDataReceive()
        {
            TestNullReceiver receiver = new TestNullReceiver();

            IPEndPoint receiveIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5274);

            int waitTime = 0, maxWaitTime = 1000, onceWaitTime = 100;

            //启动接收服务器
            new UdpServer(receiveIP, new TestLog("ReceiveServer"))
                .Run(new TestReceiversFactory(receiver));
            //启动发送服务器
            new UdpServer(new TestLog("SendServer")).Send(receiveIP, "0");

            while (!receiver.HasReceived)
            {
                Thread.Sleep(onceWaitTime);
                waitTime += onceWaitTime;
                if (waitTime >= maxWaitTime)
                {
                    break;
                }
            }

            Assert.IsTrue(receiver.HasReceived);
        }

        [TestMethod]
        public void TestDataReceive()
        {
            Console.WriteLine("0_::_::_test123456".IndexOf("_::_::_"));

            string sendData = "test123456";

            TestDataReceiver receiver = new TestDataReceiver();

            IPEndPoint receiveIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5174);

            int waitTime = 0, maxWaitTime = 1000, onceWaitTime = 100;

            //启动接收服务器
            new UdpServer(receiveIP, new TestLog("ReceiveServer"))
                .Run(new TestReceiversFactory(receiver));
            //启动发送服务器
            new UdpServer(new TestLog("SendServer")).Send(receiveIP, "0", sendData);

            while (string.IsNullOrEmpty(receiver.Content))
            {
                Thread.Sleep(onceWaitTime);
                waitTime += onceWaitTime;
                if (waitTime >= maxWaitTime)
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(receiver.Content))
            {
                Assert.Fail();
            }
            else
            {
                Assert.IsTrue(receiver.Content == sendData);
            }
        }
    }
}
