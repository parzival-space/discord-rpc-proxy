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

    RPCServer server = new RPCServer("discord-ipc-0", BasicLogger.Create<IPCServer>());

    server.OnActivityUpdate += (activity) => {
      log.LogInformation(
        "New Activity:\n" +
        JsonConvert.SerializeObject(activity, Formatting.Indented)
      );
    };

    server.OnAcceptedJoinRequest += (userId) =>
    {
      log.LogInformation($"Game accepted join request from user: {userId}");
    };

    server.OnDeclinedJoinRequest += (userId) =>
    {
      log.LogInformation($"Game declined join request from user: {userId}");
    };

    server


    System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
  }
}