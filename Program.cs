using System.Reflection;
using Dermotbg.Helpers;

namespace Dermotbg.WebServer
{
  class Program
  {
    static void Main(string[] args)
    {
      string websitePath = GetWebsitePath();
      Server.OnError = ErrorHandler;
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
    public static string ErrorHandler(Server.ServerError error)
    {
      string ret = null;
      switch (error)
      {
        case Server.ServerError.ExpiredSession:
          ret = "/ErrorPages/expiredSession.html";
          break;
        case Server.ServerError.FileNotFound:
          ret = "/ErrorPages/fileNotFound.html";
          break;
        case Server.ServerError.NotAuthorized:
          ret = "/ErrorPages/notAuthorized.html";
          break;
        case Server.ServerError.PageNotFound:
          ret = "/ErrorPages/pageNotFound.html";
          break;
        case Server.ServerError.ServerError:
          ret = "/ErrorPages/serverError.html";
          break;
        case Server.ServerError.UnknownType:
          ret = "/ErrorPages/unknownType.html";
          break;
      }
      return ret;
    }
  }
}