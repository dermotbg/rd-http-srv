using System.Runtime.InteropServices;
using System.Xml;

namespace Dermotbg.Helpers;

public static class StringHelpers 
{
  public static String LeftOf(this string input, char c)
  {
    string ret = input;
    int idx = input.IndexOf(c);
    if (idx != -1) ret = input.Substring(0, idx);
    return ret;
  }
  public static String RightOf(this string input, char c)
  {
    string ret = input;
    int idx = input.IndexOf(c);
    if (idx != -1) ret = input.Substring(idx + 1);
    return ret;
  }

  public static string RightOf(this String src, string substr)
  {
    string ret = String.Empty;
    int idx = src.IndexOf(substr);

    if (idx != -1)
    {
      ret = src.Substring(idx + substr.Length);
    }
    return ret;
  }

  public static String RightOfN(this string input, char c, int n)
  {
    string ret = String.Empty;
    int idx = -1;
    // loop through string until nth instance of char is met
    while (n > 0)
    {
      idx = input.IndexOf(c, idx + 1);
      if (idx == -1) break;
      --n; 
    }
    if (idx != -1) ret = input.Substring(idx + 1);
    return ret;
  }
  public static String[] Split(this string source, char delimeter, char quoteChar)
  {
    List<string> retArray = new List<string>();
    int start = 0, end = 0;
    bool insideField = false;

    for (end =0; end < source.Length; end++)
    {
      if (source[end] == quoteChar)
      {
        insideField = !insideField;
      }
      else if (!insideField && source[end] == delimeter)
      {
        retArray.Add(source.Substring(start, end - start));
        start = end +1;
      }
    }
    retArray.Add(source.Substring(start));
    return retArray.ToArray();
  }
}

public static class DictHelpers
{
  public static Dictionary<string, string> GetKeyValues(string data, Dictionary<string, string>? kv = null)
  {
    if (kv == null) kv = new Dictionary<string, string>();
    data.If(d => d.Length > 0, (d) => d.Split('&').ForEach(KeyValue => kv[KeyValue.LeftOf('=')] = System.Uri.UnescapeDataString(KeyValue.RightOf('='))));
    return kv;
  }
}

public static class ExtensionMethods
{
  public static bool If<T>(this T value, Func<T, bool> predicate, Action<T> action)
  {
    // method to sweeten if statements into one method
    bool ret = predicate(value);
    if (ret) action(value);
    return ret;
  }
  public static bool IfNull<T>(this T obj, Action action)
  {
    bool ret = obj == null;

    if (ret) { action(); } // calls the 'action' function passed into the mathod, i.e. callback

    return ret;
  }
  public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
  {
    foreach (var item in collection)
    {
      action(item);
    }
  }
}