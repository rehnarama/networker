using System;

namespace Network.Signalling
{
  [Serializable]
  public struct SignallingHostList
  {
    private SignallingHost[] servers;

    public SignallingHost[] Servers { get => servers; set => servers = value; }

    public void Serialize(Serializer serializer)
    {
      int length = servers?.Length ?? 0;
      serializer.SerializeInt(ref length);
      if (serializer.IsReader)
      {
        servers = new SignallingHost[length];
      }

      for (int i = 0; i < length; i++)
      {
        servers[i].Serialize(serializer);
      }
    }
  }
}