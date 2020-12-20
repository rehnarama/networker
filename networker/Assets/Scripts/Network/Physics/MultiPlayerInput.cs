using System;
using System.Linq;
using System.Collections.Generic;

namespace Network.Physics
{
  [Serializable]
  public struct MultiPlayerInput
  {
    private Dictionary<int, PlayerInput> _Inputs;
    public Dictionary<int, PlayerInput> Inputs { get => _Inputs; }

    public static MultiPlayerInput Create()
    {
      return new MultiPlayerInput()
      {
        _Inputs = new Dictionary<int, PlayerInput>()
      };
    }

    public static MultiPlayerInput Create(Dictionary<int, PlayerInput> inputs)
    {
      return new MultiPlayerInput()
      {
        _Inputs = new Dictionary<int, PlayerInput>(inputs)
      };
    }

    public PlayerInput For(int player)
    {
      if (_Inputs.TryGetValue(player, out var input))
      {
        return input;
      }
      else
      {
        return PlayerInput.Create();
      }
    }

    internal MultiPlayerInput Copy()
    {
      return MultiPlayerInput.Create(Inputs);
    }

    public static void Serialize(Serializer serializer, ref MultiPlayerInput input)
    {

      var keys = input.Inputs?.Keys?.ToArray();
      serializer.SerializeIntArray(ref keys);

      var values = input.Inputs?.Values?.ToArray() ?? new PlayerInput[keys.Length];
      for (int i = 0; i < keys.Length; i++)
      {
        PlayerInput.Serialize(serializer, ref values[i]);
      }

      if (serializer.IsReader)
      {
        input._Inputs = new Dictionary<int, PlayerInput>();
        for (int i = 0; i < keys.Length; i++)
        {
          input._Inputs[keys[i]] = values[i];

        }
      }
    }
  }
}
