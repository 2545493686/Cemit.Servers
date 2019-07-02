using System;
using System.Collections.Generic;
using System.Text;

namespace Cemit.Servers
{
    public interface IReceiversFactory
    {
        /// <summary>
        /// 根据类型返回接收器，返回null则表示类型错误
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IReceiver GetReceiver(string type);
    }
}
