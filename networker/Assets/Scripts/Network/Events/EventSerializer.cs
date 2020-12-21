using System;

namespace Network.Events
{
  public class EventSerializer
  {
    public virtual void SerializeEvents(Serializer serializer, ref IEvent[] events)
    {
      if (serializer.IsReader) {
        events = new IEvent[0];
      }
    }
  }
}