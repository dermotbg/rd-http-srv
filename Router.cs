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
    public const string POST = "post";
		public const string GET = "get";
		public const string PUT = "put";
		public const string DELETE = "delete";
    public string WebsitePath { get; set; } = null!;
    private Dictionary<string, ExtensionInfo> extFolderMap;
    public class ResponsePacket
    {
      public string Redirect { get; set; }
      public byte[] Data { get; set; }      
      public string ContentType { get; set; }
      public Encoding Encoding { get; set; }
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
      Console.WriteLine($"FileLoader fullpath pre: {fullPath}");
      string text = File.ReadAllText(fullPath);
      ResponsePacket ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };
      return ret;
    }
    //  Loader for HTML files, taking into account missing extensions and file-less ips/domains, which should default to index.html
    private ResponsePacket PageLoader(string relativePath, string ext, ExtensionInfo extInfo)
    {
      string fullPath = Path.Combine(WebsitePath, relativePath.TrimStart('/'));
      
      ResponsePacket ret;
      Console.WriteLine($"full: {fullPath} ---- websitepath: {WebsitePath} ----- ext: {ext}");
      if (Path.Combine(WebsitePath, relativePath.TrimStart('/')) == WebsitePath)
      {
        Console.WriteLine("WE HERE");
        ret = Route(GET, "/index.html", null);
        Console.WriteLine("NOT HERE");
      }
      else
      {
        if (string.IsNullOrEmpty(ext))
        {
          // add the extension ".html" if it is null
          Console.WriteLine($"EXT INSIDE ISNULLOFEMPTY{ext}");
          fullPath = fullPath + ".html";
          Console.WriteLine($"fullPath INSIDE ISNULLOFEMPTY{fullPath}");
        }
        // Inject "Pages" folder into the path
        fullPath = WebsitePath + "/Pages" + fullPath.RightOf(WebsitePath);
        Console.WriteLine($"fullPath END OF PAGELOADER{fullPath}");
        ret = FileLoader(fullPath, ext, extInfo);
      }
      return ret;
    }
    public ResponsePacket Route(string verb, string path, Dictionary<string, object>? kvParams)
    {
      string ext = Path.GetExtension(path);
      Console.WriteLine($"EXT INSIDE ROUTE{ext}");
      if(ext.Contains('.'))
      {
        ext = path.RightOf('.');
      }
      ExtensionInfo extInfo;
      ResponsePacket ret = null;
      if(extFolderMap.TryGetValue(ext, out extInfo))
      {
        string fullPath = Path.Combine(WebsitePath, path.TrimStart('/'));
        Console.WriteLine($"INSIDE IF STATEMENT{Path.Combine(WebsitePath, path)}");
        ret = extInfo.Loader(Path.Combine(WebsitePath, path), ext, extInfo);
      }
      return ret;
    }
  }
}
