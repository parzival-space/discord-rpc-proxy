// namespace space.parzival.DiscordRPCProxy.Client;

// using System;
// using System.IO.Pipes;
// using System.Text;
// using space.parzival.DiscordRPCProxy.Client.Discord.Types;

// public class NamedPipeServer
// {
//   private NamedPipeServerStream pipe;


//   public delegate void DataReceived(byte[] data);
//   public event DataReceived? OnDataReceived;


//   public NamedPipeServer(string name)
//   {
//     this.pipe = new NamedPipeServerStream(
//       name,
//       PipeDirection.InOut,
//       NamedPipeServerStream.MaxAllowedServerInstances,
//       PipeTransmissionMode.Byte,
//       PipeOptions.Asynchronous);

//     this.BeginReadPipe();
//   }


//   public void Close() {
//     this.pipe.Close();
//   }

//   private void BeginReadPipe()
//   {
//     Console.Write("Waiting for someone to connect...");
//     this.pipe.WaitForConnection();
//     Console.WriteLine("Done.");

//     byte[] headerBytes = new byte[8];
//     this.pipe.Read(headerBytes, 0, 8);

//     MessageFrameHeader header = new MessageFrameHeader(headerBytes);

//     // get message
//     byte[] message = new byte[header.GetMessageLength()];
//     this.pipe.Read(message, 0, (int)header.GetMessageLength());

//     Console.WriteLine(
//       "header:\n" +
//       $" - opcode: {header.GetOpcode()}\n" +
//       $" - chars: {header.GetMessageLength()}\n" +
//       $" - message: {Encoding.UTF8.GetString(message)}"
//     );

//     this.pipe.Close();
//   }
// }