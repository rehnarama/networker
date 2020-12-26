using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Signalling;
using Network.Signalling;

namespace signalling.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class Hosts : ControllerBase
  {
    SignallingServer signallingServer;

    private readonly ILogger<Hosts> _logger;

    public Hosts(ILogger<Hosts> logger, SignallingServer signallingServer)
    {
      this.signallingServer = signallingServer;
      _logger = logger;
    }

    [HttpGet]
    public IEnumerable<HostReturnType> Get()
    {
      return signallingServer.Hosts.Values.Select(host => new HostReturnType() { Name = host.Name, IpAddress = host.EndPoint.ToString() });
    }

    [Serializable]
    public struct HostReturnType
    {
      public string Name { get; set; }
      public string IpAddress { get; set; }
    }
  }
}
