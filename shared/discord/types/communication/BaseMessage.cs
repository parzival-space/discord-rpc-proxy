namespace RPCProxy.Shared.Discord.Types.Communication;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

#pragma warning disable CS8618
public class BaseMessage
{
  [JsonProperty("data")]
  private JObject? _data { get; set; }
  [JsonProperty("args")]
  private JObject? _args { get; set; }


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
  /// Contains ether arguments passed along with a command or
  /// data passed along with a event.
  /// </summary>
  [JsonIgnore]
  public JObject Data { 
    get {
      return 
        _data ?? 
        _args ?? 
        new JObject();
    }
    set {
      _data = value;
    }
  }
}
