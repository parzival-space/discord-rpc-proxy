namespace space.parzival.DiscordRPCProxy.Client;

using System;
using System.IO.Pipes;
using System.Text;

public class NamedPipeServerApple
{
  private NamedPipeServerStream pipe;


  public delegate void DataReceived(byte[] data);
  public event DataReceived? OnDataReceived;


  public NamedPipeServerApple(string name)
  {
    this.pipe = new NamedPipeServerStream(
      name,
      PipeDirection.InOut,
      NamedPipeServerStream.MaxAllowedServerInstances,
      PipeTransmissionMode.Byte,
      PipeOptions.Asynchronous);

    this.BeginReadPipe();
  }


  public void Close() {
    this.pipe.Close();
  }

  private void BeginReadPipe()
  {
    Console.Write("Waiting for someone to connect...");
    this.pipe.WaitForConnection();
    Console.WriteLine("Done.");

    // start reading
    Action ?beginRead = null;
    Task.Run(() =>
    {
      byte[] buffer = new byte[1024];

      beginRead = delegate
      {
        if (!this.pipe.IsConnected)
        {
          Task.Run((Action)BeginReadPipe);
          return;
        }

        try
        {
          Console.Write("Reading...");
          this.pipe.BeginRead(buffer, 0, buffer.Length, stream =>
          {
            Console.WriteLine("Done.");

            int length = this.pipe.EndRead(stream);
            if (length == 0)
            {
              Console.WriteLine("Failed.\nError: End of Stream and/or Pipe closed.");
              Thread.Sleep(100);
              this.pipe.Disconnect();
              beginRead?.Invoke();
              return;
            }

            byte[] package = new byte[length];
            Console.WriteLine($"Package contains {length} bytes.");
            Buffer.BlockCopy(buffer, 0, package, 0, length);
            buffer = new byte[buffer.Length];

            this.OnDataReceived?.Invoke(package);

            Thread.Sleep(100);
            beginRead?.Invoke();
          }, null);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
      };

      beginRead();
    });
  }
}