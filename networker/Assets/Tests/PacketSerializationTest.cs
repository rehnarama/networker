using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Network;
using Network.Packets;
using Network.Physics;

namespace Tests
{
  public class PacketSerializationTest
  {
    IPacketSerializer ps = new PacketSerializer();

    [Test]
    public void InputPacket()
    {
      var originalInput = PlayerInput.Create();
      originalInput.SetAnalog(1, 1.5f);
      originalInput.SetDigital(2, true);
      originalInput.SetDigital(3, false);

      var original = new InputPacket(originalInput, new Dictionary<int, Vector3>());
      var originalInterface = (IPacket)original;

      var writer = new Serializer(new System.IO.MemoryStream());
      ps.Serialize(writer, ref originalInterface);

      var reader = new Serializer(new System.IO.MemoryStream(writer.ToByteArray()), true);

      IPacket copyInterface = new InputPacket();
      ps.Serialize(reader, ref copyInterface);
      var copy = (InputPacket)copyInterface;

      CollectionAssert.AreEqual(original.input.AnalogInput, copy.input.AnalogInput);
      CollectionAssert.AreEqual(original.input.DigitalInput, copy.input.DigitalInput);
    }

    [Test]
    public void JoinAckPacket()
    {

      var original = new JoinAckPacket(2);
      var originalInterface = (IPacket)original;

      var writer = new Serializer(new System.IO.MemoryStream());
      ps.Serialize(writer, ref originalInterface);

      var reader = new Serializer(new System.IO.MemoryStream(writer.ToByteArray()), true);

      IPacket copyInterface = new JoinAckPacket();
      ps.Serialize(reader, ref copyInterface);
      var copy = (JoinAckPacket)copyInterface;

      Assert.AreEqual(original.playerId, copy.playerId);
    }

    [Test]
    public void PhysicsPacket()
    {
      var input1 = PlayerInput.Create();
      input1.SetAnalog(1, 1.5f);
      var input2 = PlayerInput.Create();
      input2.SetDigital(2, false);
      input2.SetDigital(3, false);

      var inputs = new MultiPlayerInput[2] {
        MultiPlayerInput.Create(),
        MultiPlayerInput.Create()
      };
      inputs[0].Inputs.Add(1, input1);
      inputs[1].Inputs.Add(2, input2);

      var states = new PhysicsState[2] {
        new PhysicsState(),
        new PhysicsState()
      };
      states[0].Id = 1;
      states[0].Velocity = new Vector3(1, 1, 1);
      states[0].Position = new Vector3(1, 1, 1);
      states[0].AngularVelocity = new Vector3(1, 1, 1);
      states[0].Rotation = new Quaternion(1, 1, 1, 0);
      states[1].Id = 2;
      states[1].Velocity = new Vector3(2, 2, 2);
      states[1].Position = new Vector3(2, 2, 2);
      states[1].AngularVelocity = new Vector3(2, 2, 2);
      states[1].Rotation = new Quaternion(2, 2, 2, 0);

      var original = new PhysicsPacket(69, inputs, states, new Network.Events.IEvent[0]);
      var originalInterface = (IPacket)original;

      var writer = new Serializer(new System.IO.MemoryStream());
      ps.Serialize(writer, ref originalInterface);

      var reader = new Serializer(new System.IO.MemoryStream(writer.ToByteArray()), true);

      IPacket copyInterface = new PhysicsPacket();
      ps.Serialize(reader, ref copyInterface);
      var copy = (PhysicsPacket)copyInterface;

      Assert.AreEqual(original.frame, copy.frame);

      CollectionAssert.AreEqual(original.inputs[0].Inputs[1].AnalogInput, copy.inputs[0].Inputs[1].AnalogInput);
      CollectionAssert.AreEqual(original.inputs[0].Inputs[1].DigitalInput, copy.inputs[0].Inputs[1].DigitalInput);
      CollectionAssert.AreEqual(original.inputs[1].Inputs[2].AnalogInput, copy.inputs[1].Inputs[2].AnalogInput);
      CollectionAssert.AreEqual(original.inputs[1].Inputs[2].DigitalInput, copy.inputs[1].Inputs[2].DigitalInput);

      Assert.AreEqual(original.states[0].Id, copy.states[0].Id);
      Assert.AreEqual(original.states[0].Position, copy.states[0].Position);
      Assert.AreEqual(original.states[0].Rotation, copy.states[0].Rotation);
      Assert.AreEqual(original.states[0].AngularVelocity, copy.states[0].AngularVelocity);
      Assert.AreEqual(original.states[0].Velocity, copy.states[0].Velocity);
      Assert.AreEqual(original.states[1].Id, copy.states[1].Id);
      Assert.AreEqual(original.states[1].Position, copy.states[1].Position);
      Assert.AreEqual(original.states[1].Rotation, copy.states[1].Rotation);
      Assert.AreEqual(original.states[1].AngularVelocity, copy.states[1].AngularVelocity);
      Assert.AreEqual(original.states[1].Velocity, copy.states[1].Velocity);
    }
  }
}
