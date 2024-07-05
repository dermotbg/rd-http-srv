using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Dermotbg.WebServer;

namespace Dermotbg.WebServer
{
  class Program
  {
    static void Main(string[] args)
    {
      string websitePath = Router.GetWebsitePath();
      Server.Start(websitePath);
      Console.ReadLine();
    }
    
  }
}