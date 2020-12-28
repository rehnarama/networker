
using Network;
using Network.Events;

namespace Events
{
  public class GameEventSerializer : EventSerializer
  {

    public override void SerializeEvents(Serializer serializer, ref IEvent[] events)
    {
      int length = events?.Length ?? 0;
      serializer.SerializeInt(ref length);

      if (serializer.IsReader)
      {
        events = new IGameEvent[length];
      }
      for (int i = 0; i < length; i++)
      {
        var e = (IGameEvent)events[i];
        SerializeEvent(serializer, ref e);
        events[i] = e;
      }
    }


    private void SerializeEvent(Serializer serializer, ref IGameEvent e)
    {
      int eventNumber = e?.EventNumber ?? 0;
      serializer.SerializeInt(ref eventNumber);

      var typeInt = (int)(e?.Type ?? 0);
      serializer.SerializeInt(ref typeInt);
      if (serializer.IsReader)
      {
        var type = (GameEvents)typeInt;

        switch (type)
        {
          case GameEvents.Instantiate:
            e = new InstantiateEvent();
            break;
          case GameEvents.LoadScene:
            e = new LoadSceneEvent();
            break;
          case GameEvents.Death:
            e = new DeathEvent();
            break;
        }

        e.EventNumber = eventNumber;
      }

      e.Serialize(serializer);
    }
  }
}