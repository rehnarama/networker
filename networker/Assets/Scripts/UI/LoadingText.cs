using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TMPro.TMP_Text))]
public class LoadingText : MonoBehaviour
{
  // Start is called before the first frame update

  private TMPro.TMP_Text textElement;

  private string originalText;

  int dots;
  float timeSinceUpdate;

  private void Awake()
  {
    textElement = GetComponent<TMPro.TMP_Text>();
    originalText = textElement.text;
  }

  // Update is called once per frame
  void Update()
  {
    timeSinceUpdate += Time.deltaTime;

    if (timeSinceUpdate > 0.5f)
    {
      dots++;
      timeSinceUpdate = 0;
    }


    var newText = originalText;
    for (int i = 0; i < dots; i++)
    {
      newText += ".";
    }

    if (newText != textElement.text)
    {
      textElement.text = newText;
    }
  }

  public void Reset()
  {
    dots = 0;
  }
}
