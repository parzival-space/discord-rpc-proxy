using Newtonsoft.Json;

namespace RPCProxy.Shared.Discord.Types.Detection
{
#pragma warning disable CS8618
  public class ThirdPartySku
  {
    [JsonProperty("distributor")]
    public string Distributor { get; set; }

    [JsonProperty("id")]
    public string ID { get; set; }

    [JsonProperty("sku")]
    public string SKU { get; set; }
  }
#pragma warning restore CS8618
}