using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ProgressBarController : MonoBehaviour
{

  public Image progressBarFull;

  [Range(0f, 1f)]
  public float progress;
  // Update is called once per frame
  void Update()
  {
    if (progressBarFull != null)
    {
      progressBarFull.fillAmount = progress;
    }
  }
}
