using System.Linq.Expressions;
using System.Net;
using System.Net.Cache;
using System.Net.Sockets;

using Dermotbg.Helpers;

namespace Dermotbg.WebServer
{
  public class Server
  {
        public enum ServerError
    {
      OK,
      ExpiredSession,
      NotAuthorized,
      FileNotFound, 
      PageNotFound,
      ServerError,
      UnknownType,
    }
    public static Func<ServerError, string> OnError { get; set; }
    private static HttpListener? listener;
    private static Router router = new Router();
    private static List<IPAddress> GetLocalHostIPs()
    {
      IPHostEntry host;
      host = Dns.GetHostEntry(Dns.GetHostName());
      List<IPAddress> ret = host.AddressList.Where(GetLocalHostIPs => GetLocalHostIPs.AddressFamily == AddressFamily.InterNetwork).ToList();
      return ret;
    }
    private static HttpListener InitializeListener(List<IPAddress> localhostIPs)
    {
      HttpListener listener = new HttpListener();
      listener.Prefixes.Add("http://localhost/");
      // listen for IPs
      localhostIPs.ForEach(ip => 
      {
        Console.WriteLine("Listening on IP " + "http://" + ip.ToString() + "/");
        listener.Prefixes.Add("http://" + ip.ToString() + "/");
      });
      return listener;
    }
    private static int maxSimultaneousConnections = 20;
    private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);
    public void AddRoute (Route route)
    {
      router.AddRoute(route);
    }

    // Being listening to connections on a separate worker thread
    private void Start(HttpListener listener)
    {
      // no need to try catch here when sudo ran 
      try
      {
        listener.Start();
        Task.Run(() => RunServer(listener));
      }
      catch (HttpListenerException ex)
      {
        Console.WriteLine($"Skipped IP due to: {ex.Message}");
      }
    }
    // Start awaiting connections up to the "maxSimultaneous value.
    private void RunServer(HttpListener listener)
    {
      while(true)
      {
        sem.WaitOne();
        StartConnectionListener(listener);
      }
    }
    // await connections
    private async void StartConnectionListener(HttpListener listener)
    {
      ResponsePacket resp;

      // await for connection. Return to caller while waiting
      HttpListenerContext context = await listener.GetContextAsync();

      // release semaphore so that another listener can be started
      sem.Release();
      Log(context.Request);
      
      HttpListenerRequest request = context.Request;
      try
      {
        string path = request.RawUrl.LeftOf("?"); // path ONLY hence "LEFTOF"
        string verb = request.HttpMethod; //Req type
        string parms = request.RawUrl.RightOf("?"); //params of url 
        // Add params to KV pairs
        Dictionary<string, object> kvParams = DictHelpers.GetKeyValues(parms);
        string data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
        DictHelpers.GetKeyValues(data, kvParams);
        Log(kvParams);
        
        resp = router.Route(verb, path, kvParams);

        if(resp.Error != ServerError.OK)
        {
          Console.WriteLine($"SERVER ERROR {resp.Error}");
          resp.Redirect = OnError(resp.Error);
          // resp = router.Route("get", OnError(resp.Error), null);
        }
        Respond(context.Request, context.Response, resp);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        resp = new ResponsePacket() { Redirect = OnError(ServerError.ServerError) };
      }
    }
    private static void Respond(HttpListenerRequest request, HttpListenerResponse response, ResponsePacket resp)
    {
      if (String.IsNullOrEmpty(resp.Redirect))
      {
        response.ContentType = resp.ContentType;
        response.ContentLength64 = resp.Data.Length;
        response.OutputStream.Write(resp.Data, 0, resp.Data.Length);
        response.ContentEncoding = resp.Encoding;
        response.StatusCode = (int)HttpStatusCode.OK;
      }
      else
      {
        response.StatusCode = (int)HttpStatusCode.Redirect;
        response.Redirect("http://" + request.UserHostAddress + resp.Redirect);
      }
      response.OutputStream.Close();
    }
    //start the server
    public void Start(string websitePath)
    {
      router.WebsitePath = websitePath;
      List<IPAddress> localhostIPs = GetLocalHostIPs();
      listener = InitializeListener(localhostIPs);
      Start(listener);
    }
    //Logger
    private void Log(HttpListenerRequest request)
    {
      Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + request?.Url?.AbsoluteUri.RightOfN('/', 3));
    }
    private void Log(Dictionary<string, object> kv)
    {
      kv.ForEach(kvp => Console.WriteLine(kvp.Key + " : " + Uri.UnescapeDataString(kvp.Value.ToString())));
    }
  }
}