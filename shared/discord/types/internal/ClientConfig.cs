using Newtonsoft.Json;

namespace RPCProxy.Shared.Discord.Types.Internal
{
  public class ClientConfig
  {
    [JsonProperty("cdn_host")]
    public string CdnHost { get; set; }

    [JsonProperty("api_endpoint")]
    public string ApiEndpoint { get; set; }

    [JsonProperty("environment")]
    public string Environment { get; set; }
    
    public static ClientConfig GetMockData() {
      return new ClientConfig()
      {
        CdnHost = "cdn.discordapp.com",
        ApiEndpoint = "//canary.discord.com/api",
        Environment = "production"
      };
    }
  }
}