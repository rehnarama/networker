

using System;

namespace Network.Packets
{
  [Serializable]
  public enum PacketType
  {
    Physics,
    Join,
    JoinAck,
    PhysicsAck,
    Input,
    EventAck
  }
}