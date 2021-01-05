using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLogController : MonoBehaviour
{
  public EventLogItemController eventLogItemPrefab;

  public void Log(string text)
  {
    var eli = Instantiate(eventLogItemPrefab, transform);
    eli.Text = text;
  }
  
}
