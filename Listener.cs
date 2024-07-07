using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

using Dermotbg.Helpers;

namespace Dermotbg.WebServer
{
  public class Server
  {
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
    // Being listening to connections on a separate worker thread
    private static void Start(HttpListener listener)
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
    private static void RunServer(HttpListener listener)
    {
      while(true)
      {
        sem.WaitOne();
        StartConnectionListener(listener);
      }
    }
    // await connections
    private static async void StartConnectionListener(HttpListener listener)
    {
      // await for connection. Return to caller while waiting
      HttpListenerContext context = await listener.GetContextAsync();
      // release semaphore so that another listener can be started
      sem.Release();
      Log(context.Request);
      HttpListenerRequest request = context.Request;
      string path = request.RawUrl.LeftOf("?"); // path ONLY hence "LEFTOF"
      string verb = request.HttpMethod; //Req type
      string parms = request.RawUrl.RightOf("?"); //params of url 
      Dictionary<string, object> kvParams = DictHelpers.GetKeyValues(parms);
      DictHelpers.DictLogger(kvParams);
      Router.ResponsePacket resp = router.Route(verb, path, kvParams);
      Respond(context.Response, resp);
    }
    private static void Respond(HttpListenerResponse response, Router.ResponsePacket resp)
    {
      // if(resp != null)
      // {
        response.ContentType = resp.ContentType;
        response.ContentLength64 = resp.Data.Length;
        response.OutputStream.Write(resp.Data, 0, resp.Data.Length);
        response.ContentEncoding = resp.Encoding;
        response.StatusCode = (int)HttpStatusCode.OK;
        response.OutputStream.Close();
      // }
      // // else{
      //   Console.WriteLine("resp is null");
      // }
    }
    //start the server
    public static void Start(string websitePath)
    {
      router.WebsitePath = websitePath;
      List<IPAddress> localhostIPs = GetLocalHostIPs();
      listener = InitializeListener(localhostIPs);
      Start(listener);
    }
    //Logger
    public static void Log(HttpListenerRequest request)
    {
      Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + request?.Url?.AbsoluteUri.RightOfN('/', 3));
    }
  }
}