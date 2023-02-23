namespace space.parzival.DiscordRPCProxy.Server;

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using RPCProxy.Shared.Communication;
using RPCProxy.Shared.Communication.Types;
using RPCProxy.Shared.Logging;

public static class Program {
  public static void Main(string[] args) {
    ILogger log = BasicLogger.Create(typeof(Program).Name);

    SingleComServer server = new SingleComServer(6969, System.Net.IPAddress.Any, logger: BasicLogger.Create<SingleComServer>(LogLevel.Information));

    server.OnDataFrameReceived += (frame) =>
    {
      log.LogInformation($"New message: {frame.Message}");
      server.Send(Opcode.Frame, $"Thank you for sending me: '{frame.Message}'");
    };

    server.OnConnectionEstablished += () =>
    {
      log.LogInformation("New client connected!");
    };

    Process.GetCurrentProcess().WaitForExit();
  }
}