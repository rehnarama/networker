using System;
using System.IO;

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

    private byte[] buffer = new byte[8]; // Max data type is 8 atm!

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

    private void SerializeByte(ref byte a)
    {
      if (IsReader)
      {
        a = (byte)Stream.ReadByte();
      }
      else
      {
        Stream.WriteByte(a);
      }
    }

    public void SerializeLong(ref long a)
    {
      if (IsReader)
      {
        Stream.Read(buffer, 0, 8);
        a = BitConverter.ToInt64(buffer, 0);
      }
      else
      {
        Stream.Write(BitConverter.GetBytes(a), 0, 8);
      }
    }
    public void SerializeChar(ref char a)
    {
      if (IsReader)
      {
        Stream.Read(buffer, 0, 2);
        a = BitConverter.ToChar(buffer, 0);
      }
      else
      {
        Stream.Write(BitConverter.GetBytes(a), 0, 2);
      }
    }

    internal void SerializeByteArray(ref byte[] a)
    {
      int length = a?.Length ?? 0;
      SerializeInt(ref length);

      if (IsReader)
      {
        a = new byte[length];
      }

      for (int i = 0; i < length; i++)
      {
        SerializeByte(ref a[i]);
      }
    }


    public void SerializeCharArray(ref char[] a)
    {
      int length = a?.Length ?? 0;
      SerializeInt(ref length);

      if (IsReader)
      {
        a = new char[length];
      }

      for (int i = 0; i < length; i++)
      {
        SerializeChar(ref a[i]);
      }
    }

    public void SerializeString(ref string a)
    {
      if (IsReader)
      {
        var arr = new char[0];
        SerializeCharArray(ref arr);
        a = new string(arr);
      }
      else
      {
        var arr = a.ToCharArray();
        SerializeCharArray(ref arr);
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

    public void SerializeVector3Array(ref float[][] a)
    {
      int length = a?.Length ?? 0;
      SerializeInt(ref length);

      if (IsReader)
      {
        a = new float[length][];
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

    public void SerializeVector3(ref float[] a)
    {
      if (IsReader)
      {
        float x = 0, y = 0, z = 0;
        SerializeFloat(ref x);
        SerializeFloat(ref y);
        SerializeFloat(ref z);

        a = new float[] { x, y, z };
      }
      else
      {
        SerializeFloat(ref a[0]);
        SerializeFloat(ref a[1]);
        SerializeFloat(ref a[2]);
      }
    }

    public void SerializeQuaternion(ref float[] a)
    {
      if (IsReader)
      {
        float x = 0, y = 0, z = 0, w = 0;
        SerializeFloat(ref x);
        SerializeFloat(ref y);
        SerializeFloat(ref z);
        SerializeFloat(ref w);

        a = new float[] { x, y, z, w };
      }
      else
      {
        SerializeFloat(ref a[0]);
        SerializeFloat(ref a[1]);
        SerializeFloat(ref a[2]);
        SerializeFloat(ref a[3]);
      }
    }

    public byte[] ToByteArray()
    {
      return Stream.ToArray();
    }

  }
}