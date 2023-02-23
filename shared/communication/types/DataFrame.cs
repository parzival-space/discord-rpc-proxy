using System.Text;

namespace RPCProxy.Shared.Communication.Types;

// totally not stolen from discord
public class DataFrame {
  public readonly DataFrameHeader Header;
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
  public DataFrame(DataFrameHeader header, byte[] message) {
    this.Header = header;
    this.Message = Encoding.UTF8.GetString(message);
    this.MessageBytes = message;

    this.VerifyFormat();
  }

  /// <summary>
  /// Creates a new MessageFrame object and automatically configures the frames header.
  /// </summary>
  /// <param name="opcode">A opcode for the current frame.</param>
  /// <param name="message">The message for this frame. Usually in json encoded.</param>
  public DataFrame(Opcode opcode, String message) {
    this.Header = new DataFrameHeader(opcode, (uint)message.Length);
    this.Message = message;
    this.MessageBytes = Encoding.UTF8.GetBytes(message);

    this.VerifyFormat();
  }

  /// <summary>
  /// Parses a MessageFrame from raw bytes.
  /// </summary>
  /// <param name="frameBytes">Raw frame bytes.</param>
  public DataFrame(byte[] frameBytes) {
    byte[] header = new byte[8];
    byte[] message = new byte[frameBytes.Length - 8];

    Array.Copy(frameBytes, 0, header, 0, 8);
    Array.Copy(frameBytes, 8, message, 0, message.Length);

    this.Header = new DataFrameHeader(header);
    this.MessageBytes = message;
    this.Message = Encoding.UTF8.GetString(message);

    this.VerifyFormat();
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


  private void VerifyFormat() {
    if (String.IsNullOrEmpty(this.Message))
      throw new FormatException("A DataFrame cannot have an empty message!");
  }
}