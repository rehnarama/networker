using System;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Events
{
  public interface IEvent
  {
    int EventNumber { get; set; }
  }

}
