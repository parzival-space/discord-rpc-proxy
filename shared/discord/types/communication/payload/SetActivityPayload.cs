using RPCProxy.Shared.Discord.Types.Internal;
using Newtonsoft.Json;

namespace RPCProxy.Shared.Discord.Types.Communication.Payload
{
#pragma warning disable CS8618
  public class SetActivityPayload
  {
    /// <summary>
    /// The ID of the process that is running the game.
    /// </summary>
    [JsonProperty("pid")]
    public int PID { get; set; }

    /// <summary>
    /// The Activity that is running.
    /// </summary>
    [JsonProperty("activity")]
    public Activity Activity { get; set; }
  }
#pragma warning restore CS8618
}