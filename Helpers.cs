namespace Dermotbg.Helpers;

public static class StringHelpers 
{
  public static String RightOfN(this String input, char c, int n)
  {
    string ret = String.Empty;
    int idx = -1;
    while (n > 0)
    {
      idx = input.IndexOf(c, idx + 1);
      if (idx == -1) break;
      --n; 
    }
    if (idx != -1) ret = input.Substring(idx + 1);
    return ret;
  }
}