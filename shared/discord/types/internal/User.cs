using Newtonsoft.Json;

namespace RPCProxy.Shared.Discord.Types.Internal
{
  public class User
  {
    [JsonProperty("id")]
    public string? ID { get; set; }

    [JsonProperty("username")]
    public string? Username { get; set; }

    [JsonProperty("discriminator")]
    public string? Discriminator { get; set; }

    [JsonProperty("avatar")]
    public string? AvatarHash { get; set; }

    [JsonProperty("avatar_decoration")]
    public string? AvatarDecorationHash { get; set; }

    [JsonProperty("bot")]
    public bool? IsBot { get; set; }

    [JsonProperty("flags")]
    public int? Flags { get; set; }

    [JsonProperty("premium_type")]
    public int? Premium { get; set; }

    public static User GetMockData() {
      return new User()
      {
        ID = "249877580180750336",
        Username = "Parzival",
        Discriminator = "9999",
        AvatarHash = "4c025c122317d2ead3588a805835e046",
        AvatarDecorationHash = null,
        IsBot = false,
        Flags = 4194464,
        Premium = 2
      };
    }
  }
}