namespace RPCProxy.Shared.Communication.Types
{
  public enum Opcode
  {
    Unknown = -1,
    Handshake = 0,
    Frame = 1,
    Close = 2,
    Ping = 3,
    Pong = 4,
    Reset = 5,
    Forward = 6,
    GameDetect = 7
  }
}