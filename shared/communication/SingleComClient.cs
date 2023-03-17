using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using RPCProxy.Shared.Communication.Types;

namespace RPCProxy.Shared.Communication
{
  public class SingleComClient
  {
    private readonly ILogger? log;
    private TcpClient client;
    private IPEndPoint endPoint;
    private int conFailedCount = 0;


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
    /// Initializes a new instance of the SingleComClient class with the specified IP address and port number,
    /// receive timeout, send timeout, and optional logger instance.
    /// </summary>
    /// <param name="address">The IP address of the remote host to connect to.</param>
    /// <param name="port">The port number of the remote host to connect to.</param>
    /// <param name="timeout">The timeout in seconds (default: 60).</param>
    /// <param name="logger">An optional logger instance to use for logging.</param>
    public SingleComClient(IPAddress address, int port, int timeout = 60, ILogger? logger = null)
    {
      this.log = logger;
      this.endPoint = new IPEndPoint(address, port);
      this.client = new TcpClient();

      this.log?.LogInformation($"Connecting to {address.ToString()}:{port}...");
      this.OnConnectionEstablished?.Invoke();

      Task.Run((Action)this.ReadLoop);
    }

    // simple setup for a new TcpClient
    private TcpClient Connect(IPEndPoint endPoint)
    {
      TcpClient client = new TcpClient();
      client.Connect(endPoint);
      return client;
    }

    // read data stream
    private void ReadLoop()
    {
      // try parse frame
      DataFrameHeader? header = null;
      DataFrame? frame = null;
      try
      {
        if (!this.client.Connected)
        {
          this.client.Close();
          this.client = Connect(this.endPoint);
          this.conFailedCount = 0; // reset connection failures.
          this.OnConnectionEstablished?.Invoke();
          this.log?.LogDebug("Connection successfully established.");
        }

        NetworkStream stream = this.client.GetStream();

        // read header
        byte[] headerBytes = new byte[8];
        int headerBytesRead = stream.Read(headerBytes, 0, 8);
        if (headerBytesRead == 0)
        {
          Thread.Sleep(100);
          this.log?.LogWarning("Unexpected communication failure.");

          this.client.Close();
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
          this.client.Close();
          this.OnConnectionClosed?.Invoke();
          ((Action)this.ReadLoop).Invoke();
          return;
        }
        uint requiredFrameBytes = header.MessageLength;

        // read message
        byte[] frameBytes = new byte[requiredFrameBytes];
        int frameBytesRead = stream.Read(frameBytes, 0, Convert.ToInt32(requiredFrameBytes));
        if (frameBytesRead != requiredFrameBytes)
        {
          Thread.Sleep(100);
          this.log?.LogWarning($"Frame data incomplete. Expected {requiredFrameBytes} bytes but only got {frameBytesRead}!");

          this.client.Close();
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
        ((Action)this.ReadLoop).Invoke();
        return;
      }
      catch (SocketException)
      {
        // connection failed unexpectedly
        if (this.conFailedCount < 10) this.conFailedCount++;

        this.log?.LogDebug($"The connection failed {this.conFailedCount} times. Waiting before reconnecting...");
        Thread.Sleep(this.conFailedCount * 1000);
      }
      catch (Exception e)
      {
        this.log?.LogError($"An unknown exception has occurred. Details will follow:");
        this.log?.LogCritical(e.ToString());
        this.log?.LogCritical(e.StackTrace);

        Thread.Sleep(100);
        this.client?.Close();
        ((Action)this.ReadLoop).Invoke();
        return;
      }

      this.ReadLoop();
    }

    private void HandleFrame(DataFrame frame)
    {
      switch (frame.Header.Opcode)
      {
        case Opcode.Ping:
          this.log?.LogDebug("Server requested heartbeat. Responding...");
          this.Send(Opcode.Pong, "{}"); // no additional data is send
          return;

        case Opcode.Pong:
          this.log?.LogWarning("Server send heartbeat response? This is not expected.");
          return;

        case Opcode.Handshake:
          // TODO: implement handshake
          return;

        case Opcode.Close:
          this.log?.LogDebug("Server requested to close connection.");
          this.client?.Close();
          return;

        default:
          this.OnDataFrameReceived?.Invoke(frame);
          return;
      }
    }

    public void Send(DataFrame frame)
    {
      if (!this.client.Connected)
      {
        this.log?.LogDebug("Tried to send message without being connected. Dropping package...");
        return;
      }

      this.client.GetStream().Write(frame.ToBytes());
    }

    public void Send(Opcode opcode, string data)
    {
      this.Send(new DataFrame(opcode, data));
    }
  }
}