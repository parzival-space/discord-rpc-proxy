using Newtonsoft.Json;

namespace RPCProxy.Shared.Discord.Types.Internal
{
  public class Configuration
  {
    [JsonProperty("cdn_host")]
    public string CdnHost { get; set; }

    [JsonProperty("api_endpoint")]
    public string ApiEndpoint { get; set; }

    [JsonProperty("environment")]
    public string Environment { get; set; }
    
    public static Configuration GetMockData() {
      return new Configuration()
      {
        CdnHost = "cdn.discordapp.com",
        ApiEndpoint = "//canary.discord.com/api",
        Environment = "production"
      };
    }
  }
}