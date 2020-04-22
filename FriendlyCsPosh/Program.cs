using System;
using System.Linq;
using System.Security;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;

namespace FriendlyCsPosh
{
  class Program
  {

    public static string ChopEnd(string source, string value)
    {
      if (!source.EndsWith(value))
        return source;

      return source.Remove(source.LastIndexOf(value));
    }

    public static bool HasWalkedBackKey(int i, string allargs, string[] keys)
    {
      return GetWalkedBackKey(i, allargs, keys).Length > 0;
    }

    public static string GetWalkedBackKey(int i, string allargs, string[] keys)
    {
      int j = i - 1;
      string curr = String.Empty;
      string rcandidate = String.Empty;
      string candidate = String.Empty;

      if (allargs.Length > 0 && j >= 0) {
        while (j >= 0 && curr != " ") {
          curr = allargs[j].ToString();
          rcandidate = rcandidate + curr;
          j = j - 1;
        }
        char[] charsToTrim = { ' ' };
        rcandidate = rcandidate.TrimEnd(charsToTrim);
        candidate = ReverseString(rcandidate);
        int pos = Array.IndexOf(keys, candidate);
        if (pos > -1) {
          return candidate;
        }
      }
      return String.Empty;

    }
    public static void ShowHelp()
    {
      Console.WriteLine("Usage:");
      Console.WriteLine("    target=somehost code=any powershell command stuff");
      Console.WriteLine("    target=somehost encoded=Base64EncodedPowershell");
      Console.WriteLine("    target=somehost code=any powershell command stuff domain=SomeDomain username=somusername password=somepassword outstring=false");

    }

    public static bool ContainsAll(Dictionary<string, string> arguments, string[] keys)
    {
      string key;
      for (int i = 0; i < keys.Length; i++) {
        key = keys[i];
        if (!arguments.ContainsKey(key)) {
          return false;
        }
      }
      return true;
    }

    public static void DebugArgs(Dictionary<string, string> arguments, string[] keys)
    {
      string key;
      string val;
      for (int i = 0; i < keys.Length; i++) {
        key = keys[i];
        if (arguments.ContainsKey(key)) {
          val = arguments[key];
          Console.WriteLine($"[+] Argument {key}: {val}");
        }
      }
    }
    public static string ReverseString(string s)
    {
      char[] charArray = s.ToCharArray();
      Array.Reverse(charArray);
      return new string(charArray);
    }

    static void Main(string[] args)
    {
      var outstring = false;
      var target = string.Empty;
      var code = string.Empty;
      var encoded = string.Empty;

      var domain = string.Empty;
      var username = string.Empty;
      var password = string.Empty;

      string[] keys = new string[] { "target", "code", "encoded", "outstring", "domain", "username", "password", "outstring" };

      var arguments = new Dictionary<string, string>();
      string allargs = String.Join(" ", args);
      string oldkey = String.Empty;

      string currkey = String.Empty;
      string currval = String.Empty;

      // mynew=friend never=believes potatoe=darkess of the soul==
      for (int i = 0; i < allargs.Length; i++) {
        if (i == allargs.Length - 1 && currkey.Length > 0 && !arguments.ContainsKey(currkey)) {
          currval = currval + allargs[i];
          arguments[currkey] = currval;
        } else if (allargs[i].ToString() == "=" && HasWalkedBackKey(i, allargs, keys)) {
          oldkey = currkey;
          currkey = GetWalkedBackKey(i, allargs, keys);
          //  Save previous if exists
          if (oldkey.Length > 0) {
            char[] charsToTrim = { ' ' };
            arguments[oldkey] = ChopEnd(currval, currkey).TrimEnd(charsToTrim);
          }
          currval = String.Empty;
        } else {
          currval = currval + allargs[i];
        }
      }

      DebugArgs(arguments, keys);

      if (arguments.ContainsKey("target")) {
        target = arguments["target"];
      }
      if (arguments.ContainsKey("code")) {
        code = arguments["code"];
      }

      if (arguments.ContainsKey("encoded")) {
        code = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(arguments["encoded"]));
      }

      if (arguments.ContainsKey("domain")) {
        domain = arguments["domain"];
      }
      if (arguments.ContainsKey("username")) {
        username = arguments["username"];
      }
      if (arguments.ContainsKey("password")) {
        password = arguments["password"];
      }
      if (arguments.ContainsKey("outstring") && arguments["outstring"].ToLower() == "true") {
        outstring = true;
      }
      try {

        if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(code)) {
          Console.WriteLine("Please include a target and code!");
          ShowHelp();
          return;
        }
      } catch (Exception e) {
        Console.Error.WriteLine(e.Message);
        ShowHelp();
        return;
      }

      try {
        var uri = new Uri($"http://{target}:5985/WSMAN");
        var conn = new WSManConnectionInfo(uri);

        if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) {
          var pass = new SecureString();
          foreach (char c in password.ToCharArray())
            pass.AppendChar(c);

          var cred = new PSCredential($"{domain}\\{username}", pass);

          conn.Credential = cred;
        }

        using (var runspace = RunspaceFactory.CreateRunspace(conn)) {
          runspace.Open();

          using (var posh = PowerShell.Create()) {
            posh.Runspace = runspace;
            posh.AddScript(code);
            if (outstring) { posh.AddCommand("Out-String"); }
            var results = posh.Invoke();
            var output = string.Join(Environment.NewLine, results.Select(R => R.ToString()).ToArray());
            Console.WriteLine(output);
          }

          runspace.Close();
        }
      } catch (Exception e) {
        Console.Error.WriteLine(e.Message);
      }
    }
  }
}