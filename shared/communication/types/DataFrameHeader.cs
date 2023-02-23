namespace RPCProxy.Shared.Communication.Types;

// totally not stolen from discord
public class DataFrameHeader {
  public readonly byte[] HeaderBytes = new byte[8];

  public readonly Opcode Opcode;
  public readonly uint MessageLength;

  /// <summary>
  /// The header of a data package send by the client.
  /// </summary>
  /// <param name="data">The raw byte data received.</param>
  public DataFrameHeader(byte[] data) {
    this.HeaderBytes = data;

    // get opcode
    if (Enum.IsDefined(typeof(Opcode), (int)this.HeaderBytes[0])) {
      this.Opcode = (Opcode)(int)this.HeaderBytes[0];
    } else {
      this.Opcode = Opcode.Unknown;
    }

    // get char count
    byte[] charCountBytes = new byte[4];
    Array.Copy(this.HeaderBytes, 4, charCountBytes, 0, 4);
    if (!BitConverter.IsLittleEndian) Array.Reverse(charCountBytes);
    this.MessageLength = BitConverter.ToUInt32(charCountBytes, 0);
  }

  /// <summary>
  /// Creates a new MessageFrameHeader. This is used to build a response.
  /// </summary>
  /// <param name="opcode"></param>
  /// <param name="length"></param>
  public DataFrameHeader(Opcode opcode, uint length) {
    this.Opcode = opcode;
    this.MessageLength = length;

    // create byte data
    byte[] headerBytes = new byte[8];
    byte[] lengthBytes = BitConverter.GetBytes(length);
    if (!BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
    headerBytes[0] = (byte)opcode;
    Array.Copy(lengthBytes, 0, headerBytes, 4, 4);
    this.HeaderBytes = headerBytes;
  }
}