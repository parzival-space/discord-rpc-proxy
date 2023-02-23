namespace RPCProxy.Shared.Discord.Types.Internal;

public class Activity
{
  public String? state { get; set; }
  public String? details { get; set; }
  public ActivityTimestamps? timestamps { get; set; }
  public ActivityAssets? assets { get; set; }
  public ActivityParty? party { get; set; }
  public ActivitySecrets? secrets { get; set; }
  public bool? instance { get; set; }
}

public class ActivityTimestamps
{
  public long? start { get; set; }
  public long? end { get; set; }
}

public class ActivityAssets
{
  public String? large_image { get; set; }
  public String? large_text { get; set; }
  public String? small_image { get; set; }
  public String? small_text { get; set; }
}

public class ActivityParty
{
  public String? id;
  public int[]? size;
}

public class ActivitySecrets
{
  public String? join;
  public String? spectate;

  // should always be null as it has been deprecated in issue #152 
  // on the original repo
  public String? match; 
}