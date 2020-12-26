
using System.Net;

public static class Config
{
  public static IPEndPoint SIGNALLING_SERVER_ENDPOINT = new IPEndPoint(IPAddress.Loopback, 1302);
  public const int SERVER_PORT = 1303;
  public const int CLIENT_PORT = 1304;
}