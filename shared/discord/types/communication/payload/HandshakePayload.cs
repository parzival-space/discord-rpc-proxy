namespace RPCProxy.Shared.Discord.Types.Communication.Payload;

using RPCProxy.Shared.Discord.Types.Internal;
using Newtonsoft.Json;

#pragma warning disable CS8618
public class ReadyMessage
{
  /// <summary>
  /// The version of the RPC.
  /// </summary>
  [JsonProperty("v")]
  public int Version { get; set; }

  /// <summary>
  /// The configuration of the connection.
  /// </summary>
  [JsonProperty("config")]
  public Configuration Configuration { get; set; }

  /// <summary>
  /// Discord User that is using the Discord Client.
  /// </summary>
  [JsonProperty("user")]
  public User User { get; set; }
}
#pragma warning restore CS8618