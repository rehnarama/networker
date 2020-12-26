using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputDialogController : MonoBehaviour
{
  public TMPro.TMP_InputField inputField;

  public Button cancelButton;
  public Button okButton;

  public bool AllowCancel
  {
    get
    {
      return cancelButton.interactable;
    }
    set
    {
      cancelButton.interactable = value;
    }
  }

  public delegate void OnInputHandler(string input);
  public event OnInputHandler OnInput;


  public void HandlePressCancel()
  {
    Destroy(gameObject);
  }

  public void HandlePressOK()
  {
    OnInput?.Invoke(inputField.text);
    Destroy(gameObject);
  }
}
