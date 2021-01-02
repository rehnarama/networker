using Network;
using Network.Events;

namespace Events
{
  public struct ReadyListItem
  {
    private int playerId;
    private bool ready;

    public int PlayerId { get => playerId; set => playerId = value; }
    public bool Ready { get => ready; set => ready = value; }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeInt(ref playerId);
      serializer.SerializeBool(ref ready);
    }
  }

  public struct ReadyListEvent : IGameEvent
  {
    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.ReadyList;

    public ReadyListItem[] readyStates;

    public void Serialize(Serializer serializer)
    {
      int length = readyStates?.Length ?? 0;
      serializer.SerializeInt(ref length);
      if (serializer.IsReader)
      {
        readyStates = new ReadyListItem[length];
      }

      for (int i = 0; i < length; i++)
      {
        readyStates[i].Serialize(serializer);
      }
    }
  }
}



