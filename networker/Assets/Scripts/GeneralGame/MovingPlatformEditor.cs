using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    MovingPlatform myTarget = (MovingPlatform)target;

    var points = serializedObject.FindProperty("points");
    if (GUILayout.Button("Add Waypoint"))
    {
      var size = points.arraySize;
      points.InsertArrayElementAtIndex(size);
      var element = points.GetArrayElementAtIndex(size);
      element.vector3Value = myTarget.transform.position;
      serializedObject.ApplyModifiedProperties();
    }


  }
}
