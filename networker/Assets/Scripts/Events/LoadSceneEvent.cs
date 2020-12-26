using Network;
using Network.Events;
using UnityEngine;

namespace Events
{
  public struct LoadSceneEvent : IGameEvent
  {
    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.LoadScene;

    private string scene;
    public string Scene { get => scene; set => scene = value; }

    public LoadSceneEvent(string scene) : this()
    {
      this.scene = scene;
    }


    public void Serialize(Serializer serializer)
    {
      serializer.SerializeString(ref scene);
    }
  }
}
