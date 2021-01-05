

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
    Input,
    EventAck,
    SignallingHost,
    SignallingHostList,
    SignallingRequestHostList,
    SignallingHolePunch,
    SignallingRequestHolePunch,
    SignallingJoinRequest,
    PhysicsAck
  }
}