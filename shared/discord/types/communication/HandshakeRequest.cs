using Newtonsoft.Json;

namespace RPCProxy.Shared.Discord.Types.Communication
{
  public class HandshakeRequest
  {
    /// <summary>
    /// The RPC version of the client.
    /// </summary>
    [JsonProperty("v")]
    public int Version = -1;

    /// <summary>
    /// The app id of the game.
    /// </summary>
    [JsonProperty("client_id")]
    public string ClientID = "1";
  }
}