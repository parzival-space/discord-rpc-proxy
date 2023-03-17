using System;
using System.IO.Pipes;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RPCProxy.Shared.Communication;
using RPCProxy.Shared.Discord;
using RPCProxy.Shared.Communication.Types;
using RPCProxy.Shared.Logging;
using RPCProxy.Shared.Communication.Messages;

namespace RPCProxy.Client
{

  public static class Program
  {

    public static void Main(string[] args)
    {
      BasicLogger log = BasicLogger.Create(typeof(Program).Name);
      log.LogInformation("Starting client...");

      // create server and tcp client
      IPCServer server = new IPCServer("discord-ipc-0", BasicLogger.Create<IPCServer>());
      SingleComClient client = new SingleComClient(IPAddress.Parse("192.168.0.40"), 21234, logger: BasicLogger.Create<SingleComClient>());
      GameDetector detector = new GameDetector(BasicLogger.Create<GameDetector>());

      server.OnConnectionClosed += () => client.Send(new DataFrame(Opcode.Reset, "{}"));
      server.OnMessageFrameReceived += (frame) =>
      {
        client.Send(new DataFrame(Opcode.Forward, frame.ToBytes()));
      };

      client.OnDataFrameReceived += (frame) =>
      {
        if (frame.Header.Opcode == Opcode.Forward) {
          log.LogInformation($"Received {frame.MessageBytes.Length} forwarded bytes from the server.");
          server.Send(new Shared.Discord.Types.MessageFrame(frame.MessageBytes));
        }
      };

      detector.Start();
      detector.OnGameStarted += (game) =>
      {
        // tell server a non-rpc game has been started
        client.Send(new DataFrame(Opcode.GameDetect, JsonConvert.SerializeObject(new GameDetectPayload(true, game))));
      };
      detector.OnGameStopped += (game) =>
      {
        // tell server a non-rpc game has been stopped
        client.Send(new DataFrame(Opcode.GameDetect, JsonConvert.SerializeObject(new GameDetectPayload(false, game))));
      };


      System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
    }
  }
}