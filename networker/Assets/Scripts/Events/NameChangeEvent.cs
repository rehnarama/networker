using Network;
using Network.Events;
using UnityEngine;

namespace Events
{
  public struct NameChangeEvent : IGameEvent
  {
    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.NameChange;

    private string newName;
    public string NewName { get => newName; set => newName = value; }

    public NameChangeEvent(string newName) : this()
    {
      this.newName = newName;
    }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeString(ref newName);
    }
  }
}


