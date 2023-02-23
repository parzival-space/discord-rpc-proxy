namespace RPCProxy.Shared.Discord.Types;

public enum Opcode
{
  Unknown = -1,
  Handshake = 0,
  Frame = 1,
  Close = 2,
  Ping = 3,
  Pong = 4
}