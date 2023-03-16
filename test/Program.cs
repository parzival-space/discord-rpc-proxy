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
    BasicLogger logger = BasicLogger.Create<GameDetector>();

    GameDetector detector = new GameDetector(logger);

    detector.Start();

    Process.GetCurrentProcess().WaitForExit();
  }

  
}