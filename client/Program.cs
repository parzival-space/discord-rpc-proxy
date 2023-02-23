namespace RPCProxy.Client;

using System;
using System.IO.Pipes;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RPCProxy.Shared.Communication;
using RPCProxy.Shared.Discord;
using RPCProxy.Shared.Discord.Types;
using RPCProxy.Shared.Discord.Types.Internal;
using RPCProxy.Shared.Logging;
using space.parzival.DiscordRPCProxy.Client;

public static class Program {

  public static void Main(string[] args) {
    BasicLogger log = BasicLogger.Create(typeof(Program).Name);

    IPCServer server = new IPCServer("discord-ipc-0", BasicLogger.Create<IPCServer>());
    SingleComClient client = new SingleComClient(System.Net.IPAddress.Parse("192.168.0.29"), 6969, logger: BasicLogger.Create<SingleComClient>());

    client.OnDataFrameReceived += (frame) =>
    {
      DataPackage? package = JsonConvert.DeserializeObject<DataPackage>(frame.Message);
      if (package == null) return;

      server.Send(package.opcode, package.message);
    };

    server.OnMessageFrameReceived += (frame) =>
    {
      client.Send(RPCProxy.Shared.Communication.Types.Opcode.Frame, JsonConvert.SerializeObject(new DataPackage() {
        opcode = frame.Header.Opcode,
        message = frame.Message
      }));
    };

    System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
  }
}