using System.Text;

namespace RPCProxy.Shared.Discord.Types;

public class MessageFrame {
  public readonly MessageFrameHeader Header;
  public readonly String Message;
  public readonly byte[] MessageBytes;

  /// <summary>
  /// Creates a new MessageFrame from the given header and the given byte array.
  /// </summary>
  /// <remarks>
  /// Dangerous constructor: It is possible to send a header that does not fit to the message array.
  /// Using this constructor with dynamic data is not advised.
  /// </remarks>
  /// <param name="header"></param>
  /// <param name="message"></param>
  public MessageFrame(MessageFrameHeader header, byte[] message) {
    this.Header = header;
    this.Message = Encoding.UTF8.GetString(message);
    this.MessageBytes = message;
  }

  /// <summary>
  /// Creates a new MessageFrame object and automatically configures the frames header.
  /// </summary>
  /// <param name="opcode">A opcode for the current frame.</param>
  /// <param name="message">The message for this frame. Usually in json encoded.</param>
  public MessageFrame(Opcode opcode, String message) {
    this.Header = new MessageFrameHeader(opcode, (uint)message.Length);
    this.Message = message;
    this.MessageBytes = Encoding.UTF8.GetBytes(message);
  }

  /// <summary>
  /// Parses a MessageFrame from raw bytes.
  /// </summary>
  /// <param name="frameBytes">Raw frame bytes.</param>
  public MessageFrame(byte[] frameBytes) {
    byte[] header = new byte[8];
    byte[] message = new byte[frameBytes.Length - 8];

    Array.Copy(frameBytes, 0, header, 0, 8);
    Array.Copy(frameBytes, 8, message, 0, message.Length);

    this.Header = new MessageFrameHeader(header);
    this.MessageBytes = message;
    this.Message = Encoding.UTF8.GetString(message);
  }

  /// <summary>
  /// Converts the MessageFrame object into raw bytes.
  /// </summary>
  /// <returns></returns>
  public byte[] ToBytes() {
    byte[] data = new byte[this.Header.HeaderBytes.Length + this.MessageBytes.Length];
    Array.Copy(this.Header.HeaderBytes, 0, data, 0, this.Header.HeaderBytes.Length);
    Array.Copy(this.MessageBytes, 0, data, this.Header.HeaderBytes.Length, this.MessageBytes.Length);
    return data;
  }
}