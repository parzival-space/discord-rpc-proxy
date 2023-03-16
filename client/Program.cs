using System;
using System.IO.Pipes;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RPCProxy.Shared.Communication;
using RPCProxy.Shared.Discord;
using RPCProxy.Shared.Discord.Types;
using RPCProxy.Shared.Discord.Types.Internal;
using RPCProxy.Shared.Logging;

namespace RPCProxy.Client
{

  public static class Program
  {

    public static void Main(string[] args)
    {
      BasicLogger log = BasicLogger.Create(typeof(Program).Name);

      // create server and tcp client
      IPCServer server = new IPCServer("discord-ipc-0", BasicLogger.Create<IPCServer>());
      SingleComClient client = new SingleComClient(IPAddress.Parse("192.168.122.1"), 1234, logger: BasicLogger.Create<SingleComClient>());



      System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
    }
  }
}