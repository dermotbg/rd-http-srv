using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Dermotbg.Helpers;
using Dermotbg.WebServer;

namespace Dermotbg.WebServer
{
  class Program
  {
    static void Main(string[] args)
    {
      string websitePath = GetWebsitePath();
      Server.Start(websitePath);
      Console.ReadLine();
    }
    public static string GetWebsitePath()
    {
      string websitePath = Assembly.GetExecutingAssembly().Location;
      Console.WriteLine($"websitePath Pre: {websitePath}");
      websitePath = websitePath.LeftOfRightmostOf("/").LeftOfRightmostOf("/").LeftOfRightmostOf("/").LeftOfRightmostOf("/") + "/Website";
      Console.WriteLine($"websitePath: {websitePath}");
      return websitePath;
    }
  }
}