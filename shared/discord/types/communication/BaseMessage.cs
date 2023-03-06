namespace RPCProxy.Shared.Discord.Types.Communication;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RPCProxy.Shared.Discord.Types.Internal;
using Newtonsoft.Json.Converters;
using RPCProxy.Shared.Discord.Utils;

#pragma warning disable CS8618
public class BaseMessage
{
  /// <summary>
  /// The command of this message.
  /// </summary>
  [JsonProperty("cmd"), JsonConverter(typeof(FallbackEnumConverter<Command>))]
  public Command Command = Command.UNKNOWN;

  /// <summary>
  /// The event of this message.
  /// </summary>
  [JsonProperty("evt"), JsonConverter(typeof(FallbackEnumConverter<Event>))]
  public Event Event = Event.UNKNOWN;

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
