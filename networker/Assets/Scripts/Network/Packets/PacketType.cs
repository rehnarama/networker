

using System;

namespace Network.Packets
{
  [Serializable]
  public enum PacketType
  {
    Noop,
    Physics,
    Join,
    JoinAck,
    PhysicsAck,
    Input,
    EventAck,
    SignallingHost,
    SignallingHostList,
    SignallingRequestHostList
  }
}