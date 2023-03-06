using Newtonsoft.Json;

namespace RPCProxy.Shared.Discord.Types.Internal
{
  public class Activity
  {
    [JsonProperty("state")]
    public String? State { get; set; }

    [JsonProperty("details")]
    public String? Details { get; set; }

    [JsonProperty("timestamps")]
    public ActivityTimestamps? Timestamps { get; set; }

    [JsonProperty("assets")]
    public ActivityAssets? Assets { get; set; }

    [JsonProperty("party")]
    public ActivityParty? Party { get; set; }

    [JsonProperty("secrets")]
    public ActivitySecrets? Secrets { get; set; }

    [JsonProperty("instance")]
    public bool? Instance { get; set; }
  }

  public class ActivityTimestamps
  {

    [JsonProperty("start")]
    public long? Start { get; set; }

    [JsonProperty("end")]
    public long? End { get; set; }
  }

  public class ActivityAssets
  {

    [JsonProperty("large_image")]
    public String? LargeImage { get; set; }

    [JsonProperty("large_text")]
    public String? LargeText { get; set; }

    [JsonProperty("small_image")]
    public String? SmallImage { get; set; }

    [JsonProperty("small_text")]
    public String? SmallText { get; set; }
  }

  public class ActivityParty
  {

    [JsonProperty("id")]
    public String? ID;

    [JsonProperty("size")]
    public int[]? Size;
  }

  public class ActivitySecrets
  {

    [JsonProperty("join")]
    public String? JoinSecret;

    [JsonProperty("spectate")]
    public String? SpectateSecret;

    /// <summary>
    /// has been deprecated in issue #152 on the original repo
    /// </summary>
    [JsonProperty("match")]
    public String? MatchSecret;
  }
}