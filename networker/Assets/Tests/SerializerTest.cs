using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Network;

namespace Tests
{
  public class SerializerTest
  {
    [Test]
    public void SerializeInt()
    {
      var writer = new Serializer(new System.IO.MemoryStream());

      int a = -10, b = 0, c = 15;

      writer.SerializeInt(ref a);
      writer.SerializeInt(ref b);
      writer.SerializeInt(ref c);

      var reader = new Serializer(new System.IO.MemoryStream(writer.ToByteArray()), true);
      int a2 = 0, b2 = 0, c2 = 0;

      reader.SerializeInt(ref a2);
      reader.SerializeInt(ref b2);
      reader.SerializeInt(ref c2);

      Assert.AreEqual(a, a2);
      Assert.AreEqual(b, b2);
      Assert.AreEqual(c, c2);
    }

    [Test]
    public void SerializeFloat()
    {
      var writer = new Serializer(new System.IO.MemoryStream());

      float a = -10.5f, b = 0f, c = 15.2f;

      writer.SerializeFloat(ref a);
      writer.SerializeFloat(ref b);
      writer.SerializeFloat(ref c);

      var reader = new Serializer(new System.IO.MemoryStream(writer.ToByteArray()), true);
      float a2 = 0, b2 = 0, c2 = 0;

      reader.SerializeFloat(ref a2);
      reader.SerializeFloat(ref b2);
      reader.SerializeFloat(ref c2);

      Assert.AreEqual(a, a2);
      Assert.AreEqual(b, b2);
      Assert.AreEqual(c, c2);
    }

    [Test]
    public void SerializeBool()
    {
      var writer = new Serializer(new System.IO.MemoryStream());

      bool a = true, b = false;

      writer.SerializeBool(ref a);
      writer.SerializeBool(ref b);

      var reader = new Serializer(new System.IO.MemoryStream(writer.ToByteArray()), true);
      bool a2 = false, b2 = false;

      reader.SerializeBool(ref a2);
      reader.SerializeBool(ref b2);

      Assert.AreEqual(a, a2);
      Assert.AreEqual(b, b2);
    }

    [Test]
    public void SerializeIntArray()
    {
      var writer = new Serializer(new System.IO.MemoryStream());

      var original = new int[3] { -5, 0, 10 };

      writer.SerializeIntArray(ref original);

      var reader = new Serializer(new System.IO.MemoryStream(writer.ToByteArray()), true);
      var copy = new int[0];

      reader.SerializeIntArray(ref copy);

      Assert.AreEqual(original, copy);
    }

    [Test]
    public void SerializeBoolArray()
    {
      var writer = new Serializer(new System.IO.MemoryStream());

      var original = new bool[3] { true, false, true };

      writer.SerializeBoolArray(ref original);

      var reader = new Serializer(new System.IO.MemoryStream(writer.ToByteArray()), true);
      var copy = new bool[0];

      reader.SerializeBoolArray(ref copy);

      Assert.AreEqual(original, copy);
    }

    [Test]
    public void SerializeFloatArray()
    {
      var writer = new Serializer(new System.IO.MemoryStream());

      var original = new float[3] { -1.2f, 0f, 10.8f };

      writer.SerializeFloatArray(ref original);

      var reader = new Serializer(new System.IO.MemoryStream(writer.ToByteArray()), true);
      var copy = new float[0];

      reader.SerializeFloatArray(ref copy);

      Assert.AreEqual(original, copy);
    }

    [Test]
    public void SerializeMultipleArrays()
    {
      var writer = Serializer.CreateWriter();

      var intOriginal = new int[3] { -5, 0, 10 };
      var boolOriginal = new bool[3] { true, false, true };

      writer.SerializeIntArray(ref intOriginal);
      writer.SerializeBoolArray(ref boolOriginal);

      var reader = Serializer.CreateReader(writer.ToByteArray());

      var boolCopy = new bool[0];
      var intCopy = new int[0];

      reader.SerializeIntArray(ref intCopy);
      reader.SerializeBoolArray(ref boolCopy);

      Assert.AreEqual(intOriginal, intCopy);
      Assert.AreEqual(boolOriginal, boolCopy);

    }
  }
}
