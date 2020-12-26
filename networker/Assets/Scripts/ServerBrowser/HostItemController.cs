using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HostItemController : MonoBehaviour
{
  public TMPro.TMP_Text text;
  public Button button;

  public string ServerName
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

  public Button.ButtonClickedEvent OnClick
  {
    get
    {
      return button.onClick;
    }
  }

}
