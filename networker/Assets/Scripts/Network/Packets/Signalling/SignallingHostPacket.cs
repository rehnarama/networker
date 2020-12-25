using System;

namespace Network.Packets.Signalling
{

  [Serializable]
  public struct SignallingHostPacket : IPacket
  {
    public PacketType Type => PacketType.SignallingHost;

    private string name;
    public string Name { get => name; set => name = value; }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeString(ref name);
    }
  }
}