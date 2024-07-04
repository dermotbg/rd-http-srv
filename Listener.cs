using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Dermotbg.Helpers;

namespace Dermotbg.WebServer
{
  public static class Server
  {
    private static HttpListener? listener;
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
      string response = "Hello Browser!";
      byte[] encoded = Encoding.UTF8.GetBytes(response);
      context.Response.ContentLength64 = encoded.Length;
      context.Response.OutputStream.Write(encoded, 0, encoded.Length);
      context.Response.OutputStream.Close();
    }
    //start the server
    public static void Start()
    {
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