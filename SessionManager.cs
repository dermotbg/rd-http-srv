using System.Net;
using System.Security.Cryptography.X509Certificates;
using Dermotbg.Helpers;

namespace Dermotbg.WebServer
{
  // sessions are assosiated with the clients IP
  public class Session
  {
    public DateTime LastConnection { get; set; }
    public bool Authorized { get; set; }
    // objects DIct is used by controllers to add additional info that needs to persist in the session
    public Dictionary<string, string> Objects { get; set; }
    public Session()
    {
      Objects = new Dictionary<string, string>();
      UpdateLastConnectionTime();
    }
    public void UpdateLastConnectionTime()
    {
      LastConnection = DateTime.Now;
    }
    public bool IsExpired(int expirationInSeconds)
    {
      return (DateTime.Now - LastConnection).TotalSeconds > expirationInSeconds;
    }
    public class SessionManager
    {
      // Tracks all sessions
      protected Dictionary<IPAddress, Session> sessionMap = new Dictionary<IPAddress, Session>();
      public SessionManager()
      {
        sessionMap = new Dictionary<IPAddress, Session>();
      }
      // Creates or returns the existing session for this remote endpoint
      public Session GetSession(IPEndPoint remoteEndPoint)
      {
        Session session = sessionMap.CreateOrGet(remoteEndPoint.Address);
        return session;
      }
    }
  }
}