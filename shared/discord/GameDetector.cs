using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Management;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RPCProxy.Shared.Discord.Types;
using RPCProxy.Shared.Discord.Types.Detection;
using RPCProxy.Shared.Discord.Utils;

namespace RPCProxy.Shared.Discord
{
  public class GameDetector
  {
    private readonly ILogger? log;
    private readonly string API_PATH = "https://discord.com/api/applications/detectable";
    private readonly string USER_AGENT = "Discord RPC Proxy (https://github.com/parzival-space/discord-rpc-proxy)";

    private ManagementEventWatcher? startWatcher;
    private ManagementEventWatcher? stopWatcher;

    private List<Game>? detectableList;

    public delegate void GameDetected(Game detectedGame);
    public event GameDetected? OnGameStarted;
    public event GameDetected? OnGameStopped;

    /// <summary>
    /// Re-implementation of Discord game detection for non-RPC games.
    /// </summary>
    /// <param name="log"></param>
    public GameDetector(ILogger? log = null) {
      this.log = log;

      if (!OperatingSystem.IsWindows()) {
        this.log?.LogWarning($"This functionality is only available on Windows.");
        return;
      }

      this.RegisterWatchers();
    }

    private List<Game> DownloadDetectableList() {
      using (HttpClient client = new HttpClient())
      using (HttpRequestMessage request = new HttpRequestMessage()) {
        // configure request
        request.RequestUri = new Uri(API_PATH);
        request.Method = HttpMethod.Get;

        // add headers
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", USER_AGENT);

        // make request
        var response = client.SendAsync(request).Result;
        if (response.IsSuccessStatusCode) {
          string jsonData = response.Content.ReadAsStringAsync().Result;
          return JsonConvert.DeserializeObject<List<Game>>(jsonData) ?? new List<Game>();
        }

        return new List<Game>();
      }
    }

    private void RegisterWatchers() {
      if (!OperatingSystem.IsWindows()) return;

      // watch started processes
      startWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", "SELECT * FROM __InstanceCreationEvent WITHIN .025 WHERE TargetInstance ISA 'Win32_Process'");
      stopWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", "SELECT * FROM __InstanceDeletionEvent WITHIN .025 WHERE TargetInstance ISA 'Win32_Process'");

      /// those are theoretically better because they wont "skip" process events, but require elevated privileges
      // startWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", "SELECT * FROM Win32_ProcessStartTrace");
      // stopWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", "SELECT * FROM Win32_ProcessStopTrace");

      // begin scan
      #pragma warning disable CA1416 // Compiler is annoying
      startWatcher.EventArrived += (s, e) =>
      {
        Game? detectedGame = this.detectableList!.FirstOrDefault(g => g!.Executables.Any(ex => (((ManagementBaseObject)e.NewEvent["TargetInstance"])["Name"].ToString() ?? "").Contains(ex.Name)), null);
        
        if (detectedGame != null) {
          this.log?.LogDebug($"Game {detectedGame.Name} with id {detectedGame.ID} has been started.");
          this.OnGameStarted?.Invoke(detectedGame);
        }
      };
      stopWatcher.EventArrived += (s, e) =>
      {
        Game? detectedGame = this.detectableList!.FirstOrDefault(g => g!.Executables.Any(ex => (((ManagementBaseObject)e.NewEvent["TargetInstance"])["Name"].ToString() ?? "").Contains(ex.Name)), null);

        if (detectedGame != null) {
          this.log?.LogDebug($"Game {detectedGame.Name} with id {detectedGame.ID} has been closed.");
          this.OnGameStopped?.Invoke(detectedGame);
        }
      };
      #pragma warning restore CA1416
    }

    /// <summary>
    /// Starts the detection service.
    /// </summary>
    public void Start() {
      if (!OperatingSystem.IsWindows()) return;

      if (this.detectableList == null) {
        this.log?.LogInformation("Downloading list of detectable games...");
        this.detectableList = this.DownloadDetectableList();
      }

      if (this.startWatcher != null) this.startWatcher.Start();
      if (this.stopWatcher != null) this.stopWatcher.Start();
    }

    /// <summary>
    /// Stops the detection service.
    /// </summary>
    public void Stop() {
      if (!OperatingSystem.IsWindows()) return;
      
      if (this.startWatcher != null) this.startWatcher.Stop();
      if (this.stopWatcher != null) this.stopWatcher.Stop();
    }
  }
}