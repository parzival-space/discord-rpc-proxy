using System;
using System.IO.Pipes;
using Microsoft.Extensions.Logging;
using RPCProxy.Shared.Discord.Types;
using RPCProxy.Shared.Discord.Utils;

namespace RPCProxy.Shared.Discord
{
  /// <summary>
  /// A client for Discords IPC protocol.
  /// </summary>
  public class IPCClient : IDisposable
  {
    private readonly ILogger? log;
    private readonly string pipeName;
    private NamedPipeClientStream pipe;


    /// <summary>
    /// Event that gets fired if a new MessageFrame get received.
    /// </summary>
    /// <param name="frame">The new MessageFrame that got received by the client.</param>
    public delegate void MessageFrameReceived(MessageFrame frame);
    public event MessageFrameReceived? OnMessageFrameReceived;

    /// <summary>
    /// Event that gets fired when a new connection to a client has been established.
    /// </summary>
    public delegate void ConnectionEstablished();
    public event ConnectionEstablished? OnConnectionEstablished;

    /// <summary>
    /// Event that gets fired when a connection to a client has been closed.
    /// </summary>
    public delegate void ConnectionClosed();
    public event ConnectionClosed? OnConnectionClosed;


    public IPCClient(string pipeName, ILogger? logger = null)
    {
      this.log = logger;

      this.pipeName = MultiPlatformUtils.GetQualifiedPipeName(pipeName);
      this.log?.LogDebug($"Using fully qualified pipe: {this.pipeName}");
      this.pipe = new NamedPipeClientStream(this.pipeName);

      // begin reader
      this.log?.LogDebug("Server started. Beginning read-loop...");
      Task.Run((Action)this.PipeReader);
    }

    private void PipeReader()
    {
      // try parse header
      MessageFrameHeader? header = null;
      MessageFrame? frame = null;

      try
      {
        if (!this.pipe.IsConnected)
        {
          this.log?.LogDebug("Waiting for new connection...");
          this.pipe = new NamedPipeClientStream(this.pipeName);
          this.pipe.Connect();
          this.OnConnectionEstablished?.Invoke();
        }

        byte[] headerBytes = new byte[8];
        int headerBytesRead = this.pipe.Read(headerBytes, 0, 8);
        if (headerBytesRead == 0)
        {
          Thread.Sleep(100);
          this.log?.LogWarning("Failed to receive Message. The stream was closed or end of stream reached.");

          this.Disconnect();
          this.OnConnectionClosed?.Invoke();
          ((Action)this.PipeReader).Invoke();
          return;
        }

        // parse header
        header = new MessageFrameHeader(headerBytes);
        uint requiredMessageBytes = header.MessageLength;

        // read message
        byte[] messageBytes = new byte[requiredMessageBytes];
        int messageBytesRead = this.pipe.Read(messageBytes, 0, Convert.ToInt32(requiredMessageBytes));

        // create MessageFrame object
        frame = new MessageFrame(header, messageBytes);

        // call event
        this.log?.LogDebug($"Got a new MessageFrame with a {header.HeaderBytes.Length} bytes header, a {frame.MessageBytes.Length} bytes message and an opcode of {header.Opcode}.");
        this.OnMessageFrameReceived?.Invoke(frame);
      }
      catch (ObjectDisposedException)
      {
        if (!this.pipe.IsConnected)
        {
          ((Action)this.PipeReader).Invoke();
          return;
        }

        // the named pipe has been disposed
        this.log?.LogWarning($"The NamedPipe has been disposed. This may be intentional.");
        return;
      }
      catch (Exception e)
      {
        // TODO: improve exception handling
        // Console.WriteLine($"Error: {e.Message}");
        // Console.WriteLine(e.StackTrace);
        this.log?.LogError($"An unknown exception has occurred. Details will follow:");
        this.log?.LogCritical(e.ToString());
        this.log?.LogCritical(e.StackTrace);

        this.Disconnect();
        this.OnConnectionClosed?.Invoke();
        ((Action)this.PipeReader).Invoke();
        return;
      }

      this.PipeReader();
    }

    /// <summary>
    /// Sends a MessageFrame to the connected client.
    /// </summary>
    /// <param name="frame">The MessageFrame to send.</param>
    public void Send(MessageFrame frame)
    {
      this.pipe.Write(frame.ToBytes());
    }

    /// <summary>
    /// Creates a new MessageFrame and sends it to the connected client.
    /// </summary>
    /// <param name="opcode">The Opcode for the Operation of the MessageFrame.</param>
    /// <param name="message">The Message that the MessageFrame contains.</param>
    public void Send(Opcode opcode, String message)
    {
      this.pipe.Write(new MessageFrame(opcode, message).ToBytes());
    }

    /// <summary>
    /// Disconnects the current connections.
    /// </summary>
    public void Disconnect()
    {
      if (this.pipe.IsConnected)
      {
        this.pipe.Close();
        this.OnConnectionClosed?.Invoke();
      }
    }

    void IDisposable.Dispose()
    {
      // disposing the underlying named pipe, should automatically dispose the read thread
      if (this.pipe.IsConnected) this.Disconnect();
      this.pipe.Dispose();
    }
  }
}