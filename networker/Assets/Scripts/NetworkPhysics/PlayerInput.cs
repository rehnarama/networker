using System;
using System.Linq;
using System.Collections.Generic;

namespace Network.Physics
{
  [Serializable]
  public struct PlayerInput
  {
    private Dictionary<int, bool> _DigitalInput;
    public Dictionary<int, bool> DigitalInput { get => _DigitalInput; }

    private Dictionary<int, float> _AnalogInput;
    public Dictionary<int, float> AnalogInput { get => _AnalogInput; }

    public static PlayerInput Create()
    {
      return new PlayerInput()
      {
        _DigitalInput = new Dictionary<int, bool>(),
        _AnalogInput = new Dictionary<int, float>()
      };
    }

    public static PlayerInput Create(Dictionary<int, bool> digitalInput, Dictionary<int, float> analogInput)
    {
      return new PlayerInput()
      {
        _DigitalInput = new Dictionary<int, bool>(digitalInput),
        _AnalogInput = new Dictionary<int, float>(analogInput)
      };
    }

    public bool GetDigital(int id)
    {
      if (DigitalInput.TryGetValue(id, out var value))
      {
        return value;
      }
      else
      {
        return false;
      }
    }

    public float GetAnalog(int id)
    {
      if (AnalogInput.TryGetValue(id, out var value))
      {
        return value;
      }
      else
      {
        return 0f;
      }
    }

    public void SetDigital(int id, bool value)
    {
      DigitalInput[id] = value;
    }

    public void SetAnalog(int id, float value)
    {
      AnalogInput[id] = value;
    }
    public void SetAnalog(string id, float value)
    {
      SetAnalog(id.GetHashCode(), value);
    }
    public float GetAnalog(string id)
    {
      return GetAnalog(id.GetHashCode());
    }

    public static void Serialize(Serializer serializer, ref PlayerInput input)
    {
      var digitalKeys = input.DigitalInput?.Keys?.ToArray();
      serializer.SerializeIntArray(ref digitalKeys);

      var digitalValues = input.DigitalInput?.Values?.ToArray();
      serializer.SerializeBoolArray(ref digitalValues);

      var analogKeys = input.AnalogInput?.Keys?.ToArray();
      serializer.SerializeIntArray(ref analogKeys);

      var analogValues = input.AnalogInput?.Values?.ToArray();
      serializer.SerializeFloatArray(ref analogValues);

      if (serializer.IsReader)
      {
        input._DigitalInput = new Dictionary<int, bool>();
        input._AnalogInput = new Dictionary<int, float>();

        for (int i = 0; i < digitalKeys.Length; i++)
        {
          input.DigitalInput[digitalKeys[i]] = digitalValues[i];
        }
        for (int i = 0; i < analogKeys.Length; i++)
        {
          input.AnalogInput[analogKeys[i]] = analogValues[i];
        }
      }
    }

    public PlayerInput Copy()
    {
      return Create(DigitalInput, AnalogInput);
    }
  }
}