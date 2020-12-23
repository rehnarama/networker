using System;
using System.IO;
using UnityEngine;

namespace Network
{

  public class Serializer
  {
    public MemoryStream Stream { get; private set; }
    public bool IsReader { get; private set; }
    public bool IsWriter
    {
      get
      {
        return !IsReader;
      }
    }

    private byte[] buffer = new byte[4]; // Max data type is 4 atm!

    public Serializer(MemoryStream stream, bool isReader = false)
    {
      this.Stream = stream;
      this.IsReader = isReader;
    }

    public static Serializer CreateReader(byte[] data)
    {
      return new Serializer(
        new MemoryStream(data),
        true
      );
    }

    public static Serializer CreateWriter()
    {
      return new Serializer(new MemoryStream(), false);
    }


    public void SerializeInt(ref int a)
    {
      if (IsReader)
      {
        Stream.Read(buffer, 0, 4);
        a = BitConverter.ToInt32(buffer, 0);
      }
      else
      {
        Stream.Write(BitConverter.GetBytes(a), 0, 4);
      }
    }

    public void SerializeBool(ref bool a)
    {
      if (IsReader)
      {
        Stream.Read(buffer, 0, 1);
        a = BitConverter.ToBoolean(buffer, 0);
      }
      else
      {
        Stream.Write(BitConverter.GetBytes(a), 0, 1);
      }
    }

    public void SerializeIntArray(ref int[] a)
    {
      int length = a?.Length ?? 0;
      SerializeInt(ref length);

      if (IsReader)
      {
        a = new int[length];
      }

      for (int i = 0; i < length; i++)
      {
        SerializeInt(ref a[i]);
      }
    }

    public void SerializeVector3Array(ref Vector3[] a)
    {
      int length = a?.Length ?? 0;
      SerializeInt(ref length);

      if (IsReader)
      {
        a = new Vector3[length];
      }

      for (int i = 0; i < length; i++)
      {
        SerializeVector3(ref a[i]);
      }
    }

    public void SerializeBoolArray(ref bool[] a)
    {
      int length = a?.Length ?? 0;
      SerializeInt(ref length);

      if (IsReader)
      {
        a = new bool[length];
      }

      for (int i = 0; i < length; i++)
      {
        SerializeBool(ref a[i]);
      }
    }

    public void SerializeFloatArray(ref float[] a)
    {
      int length = a?.Length ?? 0;
      SerializeInt(ref length);

      if (IsReader)
      {
        a = new float[length];
      }

      for (int i = 0; i < length; i++)
      {
        SerializeFloat(ref a[i]);
      }
    }

    public void SerializeFloat(ref float a)
    {
      if (IsReader)
      {
        Stream.Read(buffer, 0, 4);
        a = BitConverter.ToSingle(buffer, 0);
      }
      else
      {
        Stream.Write(BitConverter.GetBytes(a), 0, 4);
      }
    }

    public void SerializeVector3(ref Vector3 a)
    {
      if (IsReader)
      {
        float x = 0, y = 0, z = 0;
        SerializeFloat(ref x);
        SerializeFloat(ref y);
        SerializeFloat(ref z);

        a = new Vector3(x, y, z);
      }
      else
      {
        SerializeFloat(ref a.x);
        SerializeFloat(ref a.y);
        SerializeFloat(ref a.z);
      }
    }

    public void SerializeQuaternion(ref Quaternion a)
    {
      if (IsReader)
      {
        float x = 0, y = 0, z = 0, w = 0;
        SerializeFloat(ref x);
        SerializeFloat(ref y);
        SerializeFloat(ref z);
        SerializeFloat(ref w);

        a = new Quaternion(x, y, z, w);
      }
      else
      {
        SerializeFloat(ref a.x);
        SerializeFloat(ref a.y);
        SerializeFloat(ref a.z);
        SerializeFloat(ref a.w);
      }
    }

    public byte[] ToByteArray()
    {
      return Stream.ToArray();
    }

  }
}