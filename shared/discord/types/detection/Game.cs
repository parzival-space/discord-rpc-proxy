using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RPCProxy.Shared.Discord.Types.Detection
{
#pragma warning disable CS8618
  public class Game
  {
    [JsonProperty("aliases")]
    public List<string> Aliases { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("executables")]
    public List<Executable> Executables { get; set; }

    [JsonProperty("flags")]
    public int Flags { get; set; }

    [JsonProperty("hook")]
    public bool Hook { get; set; }

    [JsonProperty("icon")]
    public JValue? Icon { get; set; }

    [JsonProperty("id")]
    public string ID { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("publishers")]
    public List<Publisher> Publishers { get; set; }

    [JsonProperty("summary")]
    public string Summary { get; set; }

    [JsonProperty("third_party_skus")]
    public List<ThirdPartySku> ThirdPartySKUs { get; set; }

    [JsonProperty("type")]
    public int? Type { get; set; }

    [JsonProperty("verify_key")]
    public string VerifyKey { get; set; }
  }
#pragma warning restore CS8618
}