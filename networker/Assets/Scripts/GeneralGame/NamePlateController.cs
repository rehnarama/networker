using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TMPro.TMP_Text))]
public class NamePlateController : MonoBehaviour
{
  public Transform follow;

  public string Name
  {
    get
    {
      return text.text;
    }
    set
    {
      text.text = value;
    }
  }

  private TMPro.TMP_Text text;

  private void Awake()
  {
    text = GetComponent<TMPro.TMP_Text>();
  }


  // Update is called once per frame
  void LateUpdate()
  {
    if (follow != null)
    {
      transform.position = follow.position + Vector3.up * 3;
      transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Camera.main.transform.up);
    }
  }
}
