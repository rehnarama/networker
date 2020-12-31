namespace Network.Game
{
  public struct PlayerListItem
  {
    private int playerId;
    public int PlayerId { get => playerId; set => playerId = value; }

    private string name;
    public string Name { get => name; set => name = value; }

    public PlayerListItem(int playerId, string name) : this()
    {
      this.playerId = playerId;
      this.name = name;
    }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeInt(ref playerId);
      serializer.SerializeString(ref name);
    }
  }

  public struct PlayerList
  {
    private PlayerListItem[] players;

    public PlayerListItem[] Players { get => players; set => players = value; }

    public PlayerList(PlayerListItem[] players) : this()
    {
      this.players = players;
    }

    public void Serialize(Serializer serializer)
    {
      int length = players?.Length ?? 0;
      serializer.SerializeInt(ref length);
      if (serializer.IsReader)
      {
        players = new PlayerListItem[length];
      }

      for (int i = 0; i < length; i++)
      {
        players[i].Serialize(serializer);
      }
    }

  }
}