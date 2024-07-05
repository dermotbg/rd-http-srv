using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Xml;
using Dermotbg.Helpers;
using Dermotbg.WebServer;

namespace Dermotbg.WebServer
{
  public class Router
  {
    public string WebsitePath { get; set; } = null!;
    private Dictionary<string, ExtensionInfo> extFolderMap;
    public class ResponsePacket
    {
      public string Redirect { get; set; } = null!;
      public byte[] Data { get; set; } = null!;
      public string ContentType { get; set; } = null!;
      public Encoding Encoding { get; set; } = null!;
    }
    internal class ExtensionInfo
    {
      public string ContentType { get; set; } = null!;
      public Func<string, string, ExtensionInfo, ResponsePacket> Loader { get; set; } = null!;
    }
    public Router()
    {
      extFolderMap = new Dictionary<string, ExtensionInfo>()
      {
        {"ico", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/ico"}},
        {"png", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/png"}},
        {"jpg", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/jpg"}},
        {"gif", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/gif"}},
        {"bmp", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/bmp"}},
        {"html", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
        {"css", new ExtensionInfo() {Loader=FileLoader, ContentType="text/css"}},
        {"js", new ExtensionInfo() {Loader=FileLoader, ContentType="text/javascript"}},
        {"", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
      };
    }
    // ImageLoader Reads in an image file and returns a ResponsePacket with the raw data.
    private ResponsePacket ImageLoader (string fullPath, string ext, ExtensionInfo extInfo)
    {
      FileStream fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
      BinaryReader br = new BinaryReader(fStream);
      ResponsePacket ret = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
      br.Close();
      fStream.Close();
      return ret;
    }
    // FileLoader reads in a (basically) text file and returns a ResponsePacket with the text UTF8 encoded
    private ResponsePacket FileLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
      string text = File.ReadAllText(fullPath);
      ResponsePacket ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };
      return ret;
    }
    //  Loader for HTML files, taking into account missing extensions and file-less ips/domains, which should default to index.html
    private ResponsePacket PageLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
      Console.WriteLine($"fullpath pre: {fullPath}");
      ResponsePacket ret = new ResponsePacket();
      if (fullPath == WebsitePath)
      {
        ret = Route("GET", "/index.html", null);
      }
      else
      {
        if (String.IsNullOrEmpty(ext))
        {
          // add the extension ".html" if it is null
          fullPath = fullPath + ".html";
        }
        // Inject "Pages" folder into the path
        fullPath = WebsitePath + "/Pages" + fullPath.RightOf(WebsitePath);
        Console.WriteLine($"fullpath post: {fullPath}");

        ret = FileLoader(fullPath, ext, extInfo);
      }
      return ret;
    }
    public ResponsePacket Route(string verb, string path, Dictionary<string, string> kvParams)
    {
      string ext = path.RightOf('.');
      ExtensionInfo extInfo;
      ResponsePacket ret = null;
      if ( extFolderMap.TryGetValue(ext, out extInfo))
      {
        //  strip leading "/" <-- possibly problematic but lets see
        string fullPath = Path.Combine(WebsitePath, path);
        Console.WriteLine(fullPath);
        ret = extInfo.Loader(fullPath, ext, extInfo);
      }
      return ret;
    }
    public static string GetWebsitePath()
    {
      string websitepath = Assembly.GetExecutingAssembly().Location;
      Console.WriteLine("websitePath");
      Console.WriteLine(websitepath);
      return websitepath;
    }
  }
}
