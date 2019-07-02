using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Cemit.Servers
{
    public interface IServer
    {
        IServer Run(IReceiversFactory receiversFactor);
        IServer Send(IPEndPoint iP, string receiverType, string content = null);
    }
}
