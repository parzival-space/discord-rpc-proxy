namespace RPCProxy.Shared.Discord;

using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPCProxy.Shared.Discord.Types;
using RPCProxy.Shared.Discord.Types.Communication;
using RPCProxy.Shared.Discord.Types.Communication.Payload;
using RPCProxy.Shared.Discord.Types.Internal;

// TODO: complete this implementation
public class RPCServer {
  private readonly ILogger? log;
  private IPCServer server;
  private bool isConnected;
  public int[] SupportedVersions = { 1 };


  /// <summary>
  /// Event that gets fired when a new connection to a client has been established.
  /// </summary>
  public delegate void ConnectionEstablished(string clientId);
  public event ConnectionEstablished? OnConnectionEstablished;

  /// <summary>
  /// Event that gets fired when a connection to a client has been closed.
  /// </summary>
  public event ConnectionClosed? OnConnectionClosed;
  public delegate void ConnectionClosed();

  /// <summary>
  /// Event that gets fired when the client wants to update the activity.
  /// </summary>
  public event ActivityUpdate? OnActivityUpdate;
  public delegate void ActivityUpdate(Activity activity);


  /// <summary>
  /// Creates a new RPCServer that looks to games like a running discord instance.
  /// </summary>
  /// <param name="pipeName">The name of the pipe to use. Default should be okay. See IPCServer for more details.</param>
  /// <param name="logger">A logger instance to use.</param>
  public RPCServer(string pipeName = "discord-ipc-0", ILogger? logger = null) {
    this.server = new IPCServer(pipeName, logger);
    this.isConnected = false;
    this.log = logger;

    // handle unexpected connection close
    this.server.OnConnectionClosed += () =>
    {
      this.isConnected = false;
      this.OnConnectionClosed?.Invoke();
    };

    // handle new messages
    this.server.OnMessageFrameReceived += (frame) => {
      Console.WriteLine($"OnMessageFrame: {frame.Message}");

      try {
        this.HandleNewMessage(frame);
      } catch (Exception e) {
        this.log?.LogCritical(
          $"An unknown exception has occurred. Details will follow:\n" +
          $"{e.Message}\n" +
          $"{e.InnerException}\n" +
          $"{e.StackTrace}"
        );
      }
    };
  }

  // internally handle new message frames
  private void HandleNewMessage(MessageFrame frame) {
    switch (frame.Header.Opcode) {
      case Opcode.Handshake:
        Console.WriteLine("Handling Handshake");
        this.HandleHandshake(frame);
        break;

      case Opcode.Frame:
        this.HandleFrame(frame);
        break;

      case Opcode.Close:
        this.server.Disconnect();
        break;

      case Opcode.Ping:
        this.server.Send(Opcode.Pong, "{}"); // not tested
        break;

      case Opcode.Pong:
        this.server.Send(Opcode.Ping, "{}"); // not tested
        break;

      default:
        this.log?.LogWarning($"Unsupported MessageFrame: (Opcode: {frame.Header.Opcode}, Message: '{frame.Message}')");
        break;
    }
  }

  // Handles a handshake request for the current connection if it was not handled before.
  private void HandleHandshake(MessageFrame frame) {
    Console.WriteLine(frame.Message);
    if (this.isConnected) {
      // this case should not happen
      this.log?.LogWarning("Got a handshake request on an established connection. Connection closed.");
      this.server.Disconnect();
      return;
    }

    // check if client api version is supported
    HandshakeRequest? request = JsonConvert.DeserializeObject<HandshakeRequest>(frame.Message);
    if (request == null || !this.SupportedVersions.Contains(request.Version)) {
      this.log?.LogError("Got an invalid Handshake request. Connection closed.");
      this.server.Disconnect();
      return;
    }

    // complete handshake
    this.OnConnectionEstablished?.Invoke(request.ClientID);
    BaseMessage response = new BaseMessage()
    {
      Command = "DISPATCH",
      Event = "READY",
      Data = JObject.FromObject(new ReadyMessage()
      {
        Version = 1,
        Configuration = Configuration.GetMockData(),
        User = User.GetMockData()
      })
    };
    string responseJson = JsonConvert.SerializeObject(response);
    Console.WriteLine(responseJson);
    this.log?.LogTrace(responseJson);

    // send response
    this.server.Send(Opcode.Frame, responseJson);
  }

  // handle normal message frames
  private void HandleFrame(MessageFrame frame) {
    BaseMessage? data = JsonConvert.DeserializeObject<BaseMessage>(frame.Message);

    switch (data?.Command)
    {
      case "SET_ACTIVITY":
        #pragma warning disable CS8600, CS8602
        SetActivityPayload payload = data.Data.ToObject<SetActivityPayload>();
        #pragma warning restore CS8600, CS8602

        this.OnActivityUpdate?.Invoke(payload!.Activity);
        break;

      default:
        this.log?.LogWarning($"Missing implementation for this command: (Opcode: {frame.Header.Opcode}, Message: '{frame.Message}')");
        break;
    }
  }
}