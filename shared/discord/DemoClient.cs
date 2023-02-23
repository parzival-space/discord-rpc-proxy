namespace RPCProxy.Shared.Discord;

using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using RPCProxy.Shared.Discord.Types;

public class DemoClient {
  private NamedPipeClientStream pipe;
  private int index = 0;

  public AutoResetEvent MessageIncoming = new AutoResetEvent(false);
  public delegate void MessageFrameReceived(MessageFrame frame, NamedPipeClientStream pipe, MessageFrame dataSend);
  public event MessageFrameReceived? OnMessageFrameReceived;

  public DemoClient() {
    this.pipe = new NamedPipeClientStream("discord-ipc-0");

    // begin reader
    Task.Run((Action)this.PipeReader);
  }

  private void PipeReader() {
    if (!this.pipe.IsConnected) {
      Console.WriteLine("Not connected!");
      this.pipe.Connect();
      // this.pipe.WaitForConnection();
      // stub
    }

    if (index == this.mockData.Length) {
      Console.WriteLine("End of mock data!");
      // Environment.Exit(0);
      return;
    }

    byte[] data = this.mockData[index].ToBytes();
    Console.WriteLine($"Sending: {Encoding.UTF8.GetString(data)}");
    this.pipe.Write(data);

    // try parse header
    MessageFrameHeader ?header = null;
    MessageFrame ?frame = null;
    try {
      byte[] headerBytes = new byte[8];
      int headerBytesRead = this.pipe.Read(headerBytes, 0, 8);
      if (headerBytesRead == 0)
      {
        Console.WriteLine("Error: Failed to receive Message. The stream was closed or end of stream reached.");
        Thread.Sleep(100);
        this.pipe.Close();
        throw new Exception("Wow. That is an unknown error if I have ever seen one.");
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
      this.MessageIncoming.Set();
      this.OnMessageFrameReceived?.Invoke(frame, this.pipe, this.mockData[index]);
    } 
    catch (Exception e) {
      // TODO: improve exception handling
      Console.WriteLine($"Error: {e.Message}");
      Console.WriteLine(e.StackTrace);
      
      throw new Exception("Wow. That is an unknown error if I have ever seen one.");
    }

    // HACK: kill pipe connection when there is a close opcode
    if (header.Opcode == Opcode.Close) {
        throw new Exception("Server asked to close connection.");
    }

    index++;
    this.PipeReader();
  }

  private MessageFrame[] mockData = {
    new MessageFrame(Opcode.Handshake, "{\"v\":1,\"client_id\":\"1050888393506168902\"}"),
    new MessageFrame(Opcode.Frame, "{\"args\":{\"pid\":3388,\"activity\":{\"state\":\"Hello World\",\"details\":\"Hello World\",\"assets\":{\"large_image\":\"a\",\"large_text\":\"Hello World\",\"small_image\":\"a\"},\"instance\":false}},\"cmd\":\"SET_ACTIVITY\",\"nonce\":\"1\"}"),
  };
}