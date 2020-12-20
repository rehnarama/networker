using UnityEngine;

namespace Network
{
  public static class UnityExtensions
  {
    public static float[] ToFloatArray(this Vector3 vector)
    {
      return new float[] { vector.x, vector.y, vector.z };
    }

    public static float[] ToFloatArray(this Quaternion quaternion)
    {
      return new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
    }

    public static Vector3 ToVector3(this float[] array)
    {
      return new Vector3(array[0], array[1], array[2]);
    }

    public static Quaternion ToQuaternion(this float[] array)
    {
      return new Quaternion(array[0], array[1], array[2], array[3]);
    }
  }

}