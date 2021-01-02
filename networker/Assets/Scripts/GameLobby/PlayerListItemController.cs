using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using Network;
using Network.Game;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItemController : MonoBehaviour
{
  public TMPro.TMP_Text text;

  public Image readyImage;

  private PlayerListItem item;

  private void OnEnable()
  {
    if (NetworkState.IsClient)
    {
      NetworkState.GameClient.ReadyStatesUpdated += HandleReadyStatesUpdated;
    }
  }

  private void OnDisable()
  {
    if (NetworkState.IsClient)
    {
      NetworkState.GameClient.ReadyStatesUpdated -= HandleReadyStatesUpdated;
    }
  }

  private void HandleReadyStatesUpdated(object sender, ReadyListItem[] e)
  {
    UpdateReadyState();
  }

  private void UpdateReadyState()
  {
    if (NetworkState.IsClient)
    {
      readyImage.enabled = NetworkState.GameClient.IsReady(item.PlayerId);
    }
  }

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
      UpdateReadyState();
    }
  }
}
