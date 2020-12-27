
using System.Net;

public static class Config
{
  public static IPEndPoint SIGNALLING_SERVER_ENDPOINT = new IPEndPoint(IPAddress.Parse("165.227.146.173"), 1302);
  public const int SERVER_PORT = 1303;
  public const int CLIENT_PORT = 1304;
}