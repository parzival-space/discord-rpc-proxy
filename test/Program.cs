namespace space.parzival.DiscordRPCProxy.Client;

using System;
using RPCProxy.Shared.Logging;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using DiscordRPC;
using Microsoft.Extensions.Logging;
using RPCProxy.Shared.Communication;
using RPCProxy.Shared.Discord;
using Newtonsoft.Json;

public static class Program
{
  private static Random rnd = new Random(1337);

  public static void Main(string[] args)
  {
    ILogger log = BasicLogger.Create(typeof(Program).ToString());
    IPCClient client = new IPCClient("discord-ipc-0", BasicLogger.Create<IPCClient>());
    SingleComServer server = new SingleComServer(6969, System.Net.IPAddress.Any, logger: BasicLogger.Create<SingleComServer>());

    server.OnDataFrameReceived += (frame) =>
    {
      DataPackage? package = JsonConvert.DeserializeObject<DataPackage>(frame.Message);
      if (package == null) return;

      client.Send(package.opcode, package.message);
    };

    client.OnMessageFrameReceived += (frame) =>
    {
      server.Send(RPCProxy.Shared.Communication.Types.Opcode.Frame, JsonConvert.SerializeObject(new DataPackage() {
        opcode = frame.Header.Opcode,
        message = frame.Message
      }));
    };

    Process.GetCurrentProcess().WaitForExit();
  }
}