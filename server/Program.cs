using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPCProxy.Shared.Communication;
using RPCProxy.Shared.Communication.Messages;
using RPCProxy.Shared.Communication.Types;
using RPCProxy.Shared.Discord;
using RPCProxy.Shared.Discord.Types;
using RPCProxy.Shared.Discord.Types.Communication;
using RPCProxy.Shared.Discord.Types.Communication.Payload;
using RPCProxy.Shared.Logging;

namespace space.parzival.DiscordRPCProxy.Server
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      BasicLogger log = BasicLogger.Create(typeof(Program).Name);
      log.LogInformation("Starting server...");

      // create server and tcp client
      IPCClient client = new IPCClient("discord-ipc-0", BasicLogger.Create<IPCClient>());
      SingleComServer server = new SingleComServer(21234, System.Net.IPAddress.Any, logger: BasicLogger.Create<SingleComServer>());

      // store
      bool isMockingGame = false;
      bool mockActivitySend = false;
      // communication logic
      server.OnDataFrameReceived += (frame) =>
      {
        if (frame.Header.Opcode == RPCProxy.Shared.Communication.Types.Opcode.Reset) {
          log.LogInformation($"Client requested a reset. Closing current IPC connection...");
          client.Disconnect();
          isMockingGame = false;
          return;
        }

        if (frame.Header.Opcode == RPCProxy.Shared.Communication.Types.Opcode.Forward) {
          // ignore forward if manual mocking is running
          if (isMockingGame) {
            log.LogWarning($"Client tried to forward data but game mocking is enabled.");
            return;
          }

          log.LogInformation($"Received {frame.MessageBytes.Length} forwarded bytes from the client.");
          client.Send(new MessageFrame(frame.MessageBytes));
          return;
        }

        if (frame.Header.Opcode == RPCProxy.Shared.Communication.Types.Opcode.GameDetect) {
          GameDetectPayload payload = JsonConvert.DeserializeObject<GameDetectPayload>(frame.Message)!;

          // game end
          if (isMockingGame && !payload.Running) {
            log.LogInformation($"Client closed detectable game {payload.Game.Name}.");

            client.Disconnect();
            isMockingGame = false;
          }

          // game start
          else if (!isMockingGame && payload.Running) {
            log.LogInformation($"Client started detectable game {payload.Game.Name}.");

            client.Send(RPCProxy.Shared.Discord.Types.Opcode.Handshake, "{\"v\": 1, \"client_id\": \"" + payload.Game.ID + "\"}");
            isMockingGame = true;
            mockActivitySend = false;
          }
        }
      };


      client.OnMessageFrameReceived += (frame) =>
      {
        if (isMockingGame) {
          if (mockActivitySend) return;

          // create mock data
          BaseMessage command = new BaseMessage()
          {
            Command = Command.SET_ACTIVITY,
            Arguments = JObject.FromObject(new SetActivityPayload()
            {
              PID = Process.GetCurrentProcess().Id,
              Activity = new RPCProxy.Shared.Discord.Types.Internal.Activity()
              {
                State = null,
                Details = null,
                Timestamps = null,
                Assets = null,
                Party = null,
                Secrets = null,
                Instance = false
              }
            }),
            Nonce = "mock"
          };

          // send mock data
          client.Send(new MessageFrame(RPCProxy.Shared.Discord.Types.Opcode.Frame, JsonConvert.SerializeObject(command)));
          mockActivitySend = true;
          return;
        }

        server.Send(new DataFrame(RPCProxy.Shared.Communication.Types.Opcode.Forward, frame.ToBytes()));
      };


      Process.GetCurrentProcess().WaitForExit();
    }
  }
}