
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
          case GameEvents.Trigger:
            e = new TriggerEvent();
            break;
          case GameEvents.Kick:
            e = new KickEvent();
            break;
          case GameEvents.Destroy:
            e = new DestroyEvent();
            break;
          case GameEvents.PlayerList:
            e = new PlayerListEvent();
            break;
          case GameEvents.NameChange:
            e = new NameChangeEvent();
            break;
          case GameEvents.Ready:
            e = new ReadyEvent();
            break;
          case GameEvents.ReadyList:
            e = new ReadyListEvent();
            break;
          case GameEvents.Win:
            e = new WinEvent();
            break;
          default:
            throw new System.SystemException($"Unknown GameEvent '{type}'. Did you forget to add serialization to this event, or is serialisation/deserialisation out of sync?");
        }

        e.EventNumber = eventNumber;
      }

      e.Serialize(serializer);
    }
  }
}