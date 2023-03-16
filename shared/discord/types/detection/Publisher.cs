using Newtonsoft.Json;

namespace RPCProxy.Shared.Discord.Types.Detection
{
#pragma warning disable CS8618
  public class Publisher
  {
    [JsonProperty("id")]
    public string ID { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
  }
#pragma warning restore CS8618
}