using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDController : MonoBehaviour
{
  public JetpackBarController jetpackBarController;

  private PlayerController _player;
  public PlayerController Player
  {
    get
    {
      return _player;
    }
    set
    {
      _player = value;
      jetpackBarController.player = value;
    }
  }
}
