namespace RPCProxy.Shared.Discord.Utils
{
  public static class MultiPlatformUtils
  {
    /// <summary>
    /// Specifies if the current executing environment is UNIX based.
    /// </summary>
    public static bool IsUnix { get => Environment.OSVersion.Platform == PlatformID.Unix; }

    /// <summary>
    /// Gets the temporary directory path for the current environment.
    /// </summary>
    /// <returns>The temporary directory path.</returns>
    public static string GetTemporaryDirectory()
    {
      string[] tempVars = { "XDG_RUNTIME_DIR", "TMPDIR", "TMP", "TEMP" };
      return tempVars.Select(Environment.GetEnvironmentVariable)
        .FirstOrDefault(v => !string.IsNullOrEmpty(v), null)
        ?? "/temp";
    }

    /// <summary>
    /// Gets the OS specific pipe name.
    /// </summary>
    /// <param name="pipe">The name of the pipe.</param>
    /// <returns>The fully qualified name of the pipe.</returns>
    public static string GetQualifiedPipeName(string pipeName)
    {
      return (IsUnix) ?
        Path.Combine(GetTemporaryDirectory(), pipeName) :
        pipeName;
    }
  }
}