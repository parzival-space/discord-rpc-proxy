namespace space.parzival.DiscordRPCProxy.Client;

using System;
using RPCProxy.Shared.Logging;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using Microsoft.Extensions.Logging;
using RPCProxy.Shared.Communication;
using RPCProxy.Shared.Discord;
using RPCProxy.Shared.Discord.Types;

public class DataPackage
{
  public Opcode opcode { get; set; }
  public string message { get; set; }
}