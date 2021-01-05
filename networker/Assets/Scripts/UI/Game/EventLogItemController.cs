using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TMPro.TMP_Text))]
public class EventLogItemController : MonoBehaviour
{
  private TMPro.TMP_Text textElement;
  public int DisappearIn = 5000;

  private float spawnTime;

  public string Text
  {
    get
    {
      return textElement.text;
    }
    set
    {
      textElement.text = value;
    }
  }

  private void Awake()
  {
    textElement = GetComponent<TMPro.TMP_Text>();
  }

  void Start()
  {
    spawnTime = Time.time;
  }

  void Update()
  {
    if (Time.time - spawnTime > DisappearIn)
    {
      Destroy(gameObject);
    }
  }
}
