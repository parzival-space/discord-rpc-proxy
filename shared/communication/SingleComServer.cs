namespace RPCProxy.Shared.Communication;

using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using RPCProxy.Shared.Communication.Types;

public class SingleComServer
{
  private readonly ILogger? log;
  private readonly TcpListener listener;
  private TcpClient? client = null;
  private bool isClientAlive = false;


  public bool IsConnected { get { return this.client != null; }}


  /// <summary>
  /// Event that gets fired if a new DataFrame get received.
  /// </summary>
  /// <param name="frame">The new DataFrame that got received by the server.</param>
  public delegate void DataFrameReceived(DataFrame frame);
  public event DataFrameReceived? OnDataFrameReceived;

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


  /// <summary>
  /// Creates a new Single-Communication-Server.
  /// This is a Tcp-Server with a maximum of one connected client that allows direct communication.
  /// </summary>
  /// <param name="port">The port to listen on.</param>
  /// <param name="address">The interface to listen on.</param>
  /// <param name="heartbeatInterval">The Heartbeat interval in seconds. (Default: 10)</param>
  /// <param name="logger">A logger to use. (Default: null)</param>
  public SingleComServer(int port, IPAddress address, int heartbeatInterval = 10, ILogger? logger = null)
  {
    this.log = logger;

    this.listener = new TcpListener(address, port);
    this.listener.Start();
    this.log?.LogInformation($"Listening on {address.ToString()}:{port}");

    new Timer(this.CheckClientHeartbeat, null, heartbeatInterval * 1000, heartbeatInterval * 1000);
    Task.Run((Action)this.ReadLoop);
  }

  // reads new packages from the client stream
  private void ReadLoop()
  {
    // try parse frame
    DataFrameHeader? header = null;
    DataFrame? frame = null;
    try
    {
      if (this.client == null)
      {
        this.client = this.listener.AcceptTcpClient();
        this.OnConnectionEstablished?.Invoke();

        // a new connection is alive
        this.isClientAlive = true;
      }

      NetworkStream stream = this.client.GetStream();

      // read header
      byte[] headerBytes = new byte[8];
      int headerBytesRead = stream.Read(headerBytes, 0, 8);
      if (headerBytesRead == 0)
      {
        Thread.Sleep(100);
        this.log?.LogWarning("Unexpected communication failure.");

        this.Disconnect();
        this.OnConnectionClosed?.Invoke();
        ((Action)this.ReadLoop).Invoke();
        return;
      }

      // parse header
      header = new DataFrameHeader(headerBytes);
      if (header.Opcode == Opcode.Unknown)
      {
        Thread.Sleep(100);
        this.log?.LogWarning("Invalid Header. Received an unknown opcode. Closing connection...");

        stream.Write(new DataFrame(Opcode.Close, "Invalid Format").ToBytes());

        this.Disconnect();
        this.OnConnectionClosed?.Invoke();
        ((Action)this.ReadLoop).Invoke();
        return;
      }
      uint requiredFrameBytes = header.MessageLength;

      // read message
      byte[] frameBytes = new byte[requiredFrameBytes];
      int frameBytesRead = stream.Read(frameBytes, 0, Convert.ToInt32(requiredFrameBytes));
      if (frameBytesRead == 0)
      {
        Thread.Sleep(100);
        this.log?.LogWarning($"Frame data incomplete. Expected {requiredFrameBytes} bytes but only got {frameBytesRead}!");

        this.Disconnect();
        this.OnConnectionClosed?.Invoke();
        ((Action)this.ReadLoop).Invoke();
        return;
      }

      // parse frame
      frame = new DataFrame(header, frameBytes);

      // call event
      this.log?.LogDebug($"Got a new DataFrame with a {header.HeaderBytes.Length} bytes header, a {frame.MessageBytes.Length} bytes message and an opcode of {header.Opcode}.");
      this.HandleFrame(frame);
    }
    catch (ObjectDisposedException)
    {
      // connection closed
      this.log?.LogWarning("The connection was closed unexpectedly.");

      Thread.Sleep(100);
      this.Disconnect();
      ((Action)this.ReadLoop).Invoke();
      return;
    }
    catch (IOException e)
    {
      if (!this.isClientAlive)
      {
        // the connection did most likely not fail, it just got closed by the server
        ((Action)this.ReadLoop).Invoke();
        return;
      }

      this.log?.LogError($"An unknown io error has occurred: {e}");
      Thread.Sleep(100);
      this.Disconnect();
      ((Action)this.ReadLoop).Invoke();
      return;
    }
    catch (Exception e)
    {
      this.log?.LogError($"An unknown communication error has occurred: {e}");

      Thread.Sleep(100);
      this.Disconnect();
      ((Action)this.ReadLoop).Invoke();
      return;
    }

    this.ReadLoop();
  }

  private void Disconnect()
  {
    this.client?.Close();
    this.isClientAlive = false;
    this.client = null;
  }

  // gets called regularly to check if the current connection is still alive
  private void CheckClientHeartbeat(object? state)
  {
    if (this.client == null) return;

    try
    {
      // send heartbeat if current client is alive
      if (this.isClientAlive)
      {
        this.isClientAlive = false;
        this.Send(Opcode.Ping, "{}"); // no additional data is send
      }

      // if the client is not alive, close connection
      else
      {
        this.log?.LogWarning("The client is not responding. Closing the connection...");
        this.Disconnect();
        this.OnConnectionClosed?.Invoke();
      }
    }
    catch (Exception e)
    {
      this.log?.LogError($"An error occurred while checking the client heartbeat: {e}");
    }
  }

  // handle each frame
  private void HandleFrame(DataFrame frame)
  {
    switch (frame.Header.Opcode)
    {
      case Opcode.Ping:
        this.log?.LogWarning("Client send heartbeat requested? This is not expected.");
        return;

      case Opcode.Pong:
        this.log?.LogDebug("Client responded to heartbeat.");
        this.isClientAlive = true;
        return;

      case Opcode.Frame:
        this.OnDataFrameReceived?.Invoke(frame);
        return;

      case Opcode.Handshake:
        // TODO: implement handshake
        return;

      case Opcode.Close:
        this.log?.LogDebug("Client requested to close connection.");
        this.Disconnect();
        this.OnConnectionClosed?.Invoke();
        return;

      default:
        this.log?.LogWarning($"Unsupported opcode in current frame: {frame.Header.Opcode}");
        return;
    }
  }

  /// <summary>
  /// Sends a new DataFrame to the current connected client.
  /// </summary>
  /// <param name="frame">The DataFrame instance to send.</param>
  public void Send(DataFrame frame)
  {
    if (this.client == null)
    {
      this.log?.LogWarning("Tried to send a message, but no client is connected. Dropping package...");
      return;
    }

    this.client.GetStream().Write(frame.ToBytes());
  }

  /// <summary>
  /// Sends a new message to the client.
  /// </summary>
  /// <param name="opcode">The opcode of the message.</param>
  /// <param name="message">The message to send.</param>
  public void Send(Opcode opcode, string message)
  {
    this.Send(new DataFrame(opcode, message));
  }

}