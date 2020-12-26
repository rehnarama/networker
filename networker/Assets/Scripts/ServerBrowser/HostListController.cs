using System.Collections;
using System.Collections.Generic;
using System.Net;
using Network.Signalling;
using UnityEngine;

public class HostListController : MonoBehaviour
{
  public HostItemController hostItemPrefab;
  public GameObject scrollContent;

  public delegate void OnJoinHandler(IPEndPoint endPoint);
  public event OnJoinHandler OnJoin;

  private SignallingHostList hostList;
  public SignallingHostList HostList
  {
    get
    {
      return hostList;
    }
    set
    {
      hostList = value;
      UpdateUI();
    }
  }

  private void UpdateUI()
  {
    foreach (Transform child in scrollContent.transform)
    {
      Destroy(child.gameObject);
    }

    foreach (var host in hostList.Servers)
    {
      var hostItem = Instantiate(hostItemPrefab, scrollContent.transform);
      hostItem.ServerName = host.Name + $" ({host.EndPoint.ToString()})";
      hostItem.OnClick.AddListener(() =>
      {
        OnJoin?.Invoke(host.EndPoint);
      });
    }
  }
}
