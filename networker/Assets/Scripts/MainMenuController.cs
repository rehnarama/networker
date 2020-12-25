using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using Network;
using TMPro;
using Events;

public class MainMenuController : MonoBehaviour
{
  public TMP_InputField inputField;

  private void Start()
  {
    inputField.text = IPAddress.Loopback.ToString();
  }

  public void StartGame(string role)
  {
    // TODO: create and initialize an eventSerializer for this game here and pass to StartPhysics*

    var gameEventSerializer = new GameEventSerializer();
    var packetSerializer = new PacketSerializer();
    if (role == "server" || role == "host")
    {
      NetworkState.StartPhysicsServer(packetSerializer, gameEventSerializer);
    }
    if (role == "client" || role == "host")
    {
      NetworkState.StartPhysicsClient(new IPEndPoint(IPAddress.Parse(inputField.text), Server.PORT), packetSerializer, gameEventSerializer);
    }


    SceneManager.LoadScene("SampleScene");
  }
}
