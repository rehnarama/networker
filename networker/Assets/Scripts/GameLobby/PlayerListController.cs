using System.Collections;
using System.Collections.Generic;
using Network.Game;
using UnityEngine;

public class PlayerListController : MonoBehaviour
{
  public PlayerListItemController playerListItemPrefab;

  public GameObject scrollContent;


  private PlayerList list;

  public PlayerList List
  {
    get => list;
    set
    {
      list = value;
      UpdateUI();
    }
  }

  private void UpdateUI()
  {
    foreach (Transform child in scrollContent.transform)
    {
      Destroy(child.gameObject);
    }

    foreach (var player in list.Players)
    {
      var hostItem = Instantiate(playerListItemPrefab, scrollContent.transform);
      hostItem.Item = player;
    }
  }
}
