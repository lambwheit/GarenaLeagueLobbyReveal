using System;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeagueTool
{
    
    internal class Program
    {
        public static string Substring(string @this, string from = null, string until = null, StringComparison comparison = StringComparison.InvariantCulture)
        {
            var fromLength = (from ?? string.Empty).Length;
            var startIndex = !string.IsNullOrEmpty(from)
                ? @this.IndexOf(from, comparison) + fromLength
                : 0;

            if (startIndex < fromLength) { throw new ArgumentException("from: Failed to find an instance of the first anchor"); }

            var endIndex = !string.IsNullOrEmpty(until)
                ? @this.IndexOf(until, startIndex, comparison)
                : @this.Length;

            if (endIndex < 0) { throw new ArgumentException("until: Failed to find an instance of the last anchor"); }

            var subString = @this.Substring(startIndex, endIndex - startIndex);
            return subString;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        private static string LCUget(string url,string port,string token)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += delegate
                {
                    return true;
                };
                var request = (HttpWebRequest)WebRequest.Create("https://127.0.0.1:" + port + url);
                request.PreAuthenticate = true;
                request.ContentType = "application/json";
                request.Method = "GET";
                string b64token = Base64Encode("riot:" + token);
                request.Headers.Add("Authorization", "Basic " + b64token);
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream(),Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
                

            }
            catch
            {
                return "";
            }
        }

        public static string clientport = "";
        public static string clienttoken = "";
        public static string riottoken = "";
        public static string riotport = "";
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            string commandline = "";
            ManagementClass mngmtClass = new ManagementClass("Win32_Process");
            bool found = false;
            while (true)
            {
                foreach (ManagementObject o in mngmtClass.GetInstances())
                {
                    if (o["Name"].Equals("LeagueClientUx.exe"))
                    {
                        commandline = o["CommandLine"].ToString();
                        found = true;
                    }
                }
                if(found)
                {break;}
            }
            //Console.WriteLine(commandline);
            riotport = Substring(commandline, "--riotclient-app-port=","\"");
            riottoken = Substring(commandline, "--riotclient-auth-token=", "\"");
            clientport = Substring(commandline, "--app-port=", "\"");
            clienttoken = Substring(commandline, "--remoting-auth-token=", "\"");
            while (true)
            {
                string lcudata = LCUget("/chat/v5/participants/champ-select", riotport, riottoken);
                
                if (lcudata != "{\"participants\":[]}")
                {
                    Console.WriteLine("In Champ Select");
                    try
                    {
                        JArray array = JArray.Parse(JObject.Parse(lcudata)["participants"].ToString());
                        if (array.Count != 5)
                        {
                            Console.Clear();
                            break;
                        }
                        foreach (var token in array)
                        {
                            //if puuid is in int/dodge list, print name as red
                            Console.WriteLine(token["name"] + " - puuid: " + token["puuid"]);
                        }

                        while (true)
                        {
                            if (LCUget("/chat/v5/participants/champ-select", riotport, riottoken) ==
                                "{\"participants\":[]}")
                            {
                                Console.Clear();
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }

                }
            }
        }
    }
}
