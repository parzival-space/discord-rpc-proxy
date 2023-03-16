using Newtonsoft.Json;

namespace RPCProxy.Shared.Discord.Types.Detection
{
#pragma warning disable CS8618
  public class Executable
  {
    [JsonProperty("is_launcher")]
    public string IsLauncher { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("os")]
    public string OS { get; set; }
  }
#pragma warning restore CS8618
}