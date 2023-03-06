using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPCProxy.Shared.Discord.Types;
using RPCProxy.Shared.Discord.Types.Communication;
using RPCProxy.Shared.Discord.Types.Communication.Payload;
using RPCProxy.Shared.Discord.Types.Internal;

namespace RPCProxy.Shared.Discord
{
  // TODO: complete this implementation
  public class RPCServer
  {
    private readonly ILogger? log;
    private IPCServer server;
    private bool isConnected;
    private HashSet<Event> subscribedEvents;
    private Activity? currentActivity;

    public readonly int[] SupportedVersions = { 1 };


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
    /// Event that gets fired when the client accepts a join request.
    /// </summary>
    public event AcceptedJoinRequest? OnAcceptedJoinRequest;
    public delegate void AcceptedJoinRequest(string userId);

    /// <summary>
    /// Event that gets fired when the client accepts a join request.
    /// </summary>
    public event DeclinedJoinRequest? OnDeclinedJoinRequest;
    public delegate void DeclinedJoinRequest(string userId);


    /// <summary>
    /// Creates a new RPCServer that looks to games like a running discord instance.
    /// </summary>
    /// <param name="pipeName">The name of the pipe to use. Default should be okay. See IPCServer for more details.</param>
    /// <param name="logger">A logger instance to use.</param>
    public RPCServer(string pipeName = "discord-ipc-0", ILogger? logger = null)
    {
      this.server = new IPCServer(pipeName, logger);
      this.subscribedEvents = new HashSet<Event>();
      this.currentActivity = null;
      this.isConnected = false;
      this.log = logger;

      // handle unexpected connection close
      this.server.OnConnectionClosed += () =>
      {
        this.isConnected = false;
        this.subscribedEvents.Clear();
        this.OnConnectionClosed?.Invoke();
      };

      // handle new messages
      this.server.OnMessageFrameReceived += (frame) =>
      {
        try
        {
          this.HandleNewMessage(frame);
        }
        catch (Exception e)
        {
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
    private void HandleNewMessage(MessageFrame frame)
    {
      switch (frame.Header.Opcode)
      {
        case Opcode.Handshake:
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
    private void HandleHandshake(MessageFrame frame)
    {
      if (this.isConnected)
      {
        // this case should not happen
        this.log?.LogWarning("Got a handshake request on an established connection. Connection closed.");
        this.server.Disconnect();
        return;
      }

      // check if client api version is supported
      HandshakeRequest? request = JsonConvert.DeserializeObject<HandshakeRequest>(frame.Message);
      if (request == null || !this.SupportedVersions.Contains(request.Version))
      {
        this.log?.LogError("Got an invalid Handshake request. Connection closed.");
        this.server.Disconnect();
        return;
      }

      // complete handshake
      this.OnConnectionEstablished?.Invoke(request.ClientID);
      BaseMessage response = new BaseMessage()
      {
        Command = Command.DISPATCH,
        Event = Event.READY,
        Data = JObject.FromObject(new ReadyMessage()
        {
          Version = 1,
          Configuration = ClientConfig.GetMockData(),
          User = User.GetMockData()
        })
      };

      // send response
      this.server.Send(Opcode.Frame, JsonConvert.SerializeObject(response));
    }

    // handle normal message frames
    private void HandleFrame(MessageFrame frame)
    {
      BaseMessage? data = JsonConvert.DeserializeObject<BaseMessage>(frame.Message);

      switch (data?.Command)
      {
        case Command.SET_ACTIVITY:
          SetActivityPayload setActivityPayload = data.Arguments!.ToObject<SetActivityPayload>() ?? new SetActivityPayload();
          this.OnActivityUpdate?.Invoke(setActivityPayload.Activity);
          break;

        case Command.SUBSCRIBE:
          Event targetEvent = data.Event;
          if (targetEvent == Event.UNKNOWN)
          {
            this.log?.LogWarning($"Client tried to subscribe to a unknown event. Original payload: {frame.Message}");
          }
          else if (!this.subscribedEvents.Contains(targetEvent))
          {
            this.subscribedEvents.Add(targetEvent);
            this.log?.LogDebug($"Subscribed to following event: {targetEvent}");
          }
          break;

        case Command.SEND_ACTIVITY_JOIN_INVITE:
          if (!this.subscribedEvents.Contains(Event.ACTIVITY_JOIN_REQUEST)) break;
          ActivityRequestPayload acceptPayload = data.Data!.ToObject<ActivityRequestPayload>() ?? new ActivityRequestPayload();
          this.OnAcceptedJoinRequest?.Invoke(acceptPayload.User);
          break;

        case Command.CLOSE_ACTIVITY_REQUEST:
          if (!this.subscribedEvents.Contains(Event.ACTIVITY_JOIN_REQUEST)) break;
          ActivityRequestPayload declinePayload = data.Data!.ToObject<ActivityRequestPayload>() ?? new ActivityRequestPayload();
          this.OnDeclinedJoinRequest?.Invoke(declinePayload.User);
          break;

        default:
          this.log?.LogWarning($"Missing implementation for this command: (Opcode: {frame.Header.Opcode}, Message: '{frame.Message}')");
          break;
      }
    }

    public Activity? GetCurrentActivity() => this.currentActivity;
  }
}