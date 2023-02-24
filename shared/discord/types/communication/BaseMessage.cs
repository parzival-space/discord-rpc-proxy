namespace RPCProxy.Shared.Discord.Types.Communication;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

#pragma warning disable CS8618
public class BaseMessage
{
  /// <summary>
  /// The command of this message.
  /// </summary>
  [JsonProperty("cmd")]
  public string? Command { get; set; }

  /// <summary>
  /// The event of this message.
  /// </summary>
  [JsonProperty("evt")]
  public string? Event { get; set; }

  /// <summary>
  /// A incremental value to help identify payloads.
  /// </summary>
  [JsonProperty("nonce")]
  public string? Nonce { get; set; }
  
  /// <summary>
  /// Event data send along with the event.
  /// (Only used when the Message is a event.)
  /// </summary>
  [JsonProperty("data")]
  public JObject? Data { get; set; }

  /// <summary>
  /// Command arguments send along with the command.
  /// (Only used when the Message is a command.)
  /// </summary>
  [JsonProperty("args")]
  public JObject? Arguments { get; set; }
}
