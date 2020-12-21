using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssignNetworkBodyId
{
  [MenuItem("Tools/Assign NetworkBody Id")]
  private static void AssignBodyid()
  {
    int id = 1; // 0 means unassigned, let's start at 1
    var bodies = GameObject.FindObjectsOfType<NetworkedBody>();

    var takenIds = new HashSet<int>();

    foreach (var body in bodies)
    {
      if (body.id != 0)
      {
        takenIds.Add(body.id);
      }
    }

    foreach (var body in bodies)
    {
      if (body.id == 0)
      {
        while (takenIds.Contains(id))
        {
          id++;
        }
        var serialisedBody = new SerializedObject(body);
        serialisedBody.FindProperty("id").intValue = id;
        serialisedBody.ApplyModifiedProperties();
        takenIds.Add(id);
      }
    }
  }
}
