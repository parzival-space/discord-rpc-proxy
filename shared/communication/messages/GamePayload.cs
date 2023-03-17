using Newtonsoft.Json;
using RPCProxy.Shared.Discord.Types.Detection;

namespace RPCProxy.Shared.Communication.Messages
{
  public class GameDetectPayload
  {
    [JsonProperty("running")]
    public bool Running { get; set; }

    [JsonProperty("game")]
    public Game Game { get; set; }

    public GameDetectPayload(bool running, Game game) {
      this.Running = running;
      this.Game = game;
    }
  }
}