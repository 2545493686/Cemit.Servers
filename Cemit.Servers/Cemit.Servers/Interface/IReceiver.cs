using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Cemit.Servers
{
    public interface IReceiver
    {
        /// <summary>
        /// 接收消息并返回回执， 返回null则不回执
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        string Receive(IPEndPoint ip, string content);
    }
}
