using RPCProxy.Shared.Discord.Types.Internal;
using Newtonsoft.Json;

namespace RPCProxy.Shared.Discord.Types.Communication.Payload
{

#pragma warning disable CS8618
  public class ActivityRequestPayload
  {
    /// <summary>
    /// The ID of the process that is running the game.
    /// </summary>
    [JsonProperty("user_id")]
    public string User { get; set; }
  }
#pragma warning restore CS8618
}