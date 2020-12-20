using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using Network;
using TMPro;

public class MainMenuController : MonoBehaviour
{
  public TMP_InputField inputField;

  private void Start()
  {
    inputField.text = IPAddress.Loopback.ToString();
  }

  public void StartGame(string role)
  {
    if (role == "server")
    {
      NetworkState.StartPhysicsServer();
    }
    else if (role == "client")
    {
      NetworkState.StartPhysicsClient(new IPEndPoint(IPAddress.Parse(inputField.text), Server.PORT));
    }
    else if (role == "host")
    {
      NetworkState.StartPhysicsServer();
      NetworkState.StartPhysicsClient(new IPEndPoint(IPAddress.Parse(inputField.text), Server.PORT));
    }
    else
    {
      return;
    }

    SceneManager.LoadScene("SampleScene");
  }
}
