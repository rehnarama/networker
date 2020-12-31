using System.Collections;
using System.Collections.Generic;
using Network;
using Network.Game;
using UnityEngine;

public class PlayerListItemController : MonoBehaviour
{
  public TMPro.TMP_Text text;

  private PlayerListItem item;

  public PlayerListItem Item
  {
    get => item;
    set
    {
      item = value;
      text.text = $"{value.PlayerId}. {value.Name}";
      if (NetworkState.PlayerId == value.PlayerId)
      {
        text.text += " (You)";
      }
    }
  }
}
