using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network.Packets
{
  using Physics;

  [Serializable]
  public struct InputPacket : IPacket
  {
    public PacketType Type => PacketType.Input;


    public PlayerInput input;

    private Dictionary<int, Vector3> _AuthorityPositions;
    public Dictionary<int, Vector3> AuthorityPositions { get => _AuthorityPositions; set => _AuthorityPositions = value; }
    public InputPacket(PlayerInput input, Dictionary<int, Vector3> authorityPositions)
    {
      this.input = input;
      this._AuthorityPositions = authorityPositions;
    }

    public void Serialize(Serializer serializer)
    {
      PlayerInput.Serialize(serializer, ref input);

      var positionsKeys = _AuthorityPositions?.Keys?.ToArray();
      var positionsValues = _AuthorityPositions?.Values?.ToArray();

      serializer.SerializeIntArray(ref positionsKeys);
      serializer.SerializeVector3Array(ref positionsValues);

      if (serializer.IsReader)
      {
        _AuthorityPositions = new Dictionary<int, Vector3>();
        for (int i = 0; i < positionsKeys.Length; i++)
        {
          _AuthorityPositions[positionsKeys[i]] = positionsValues[i];
        }
      }
    }
  }
}

