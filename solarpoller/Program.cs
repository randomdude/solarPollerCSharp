using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace solarpoller
{
    class Program
    {
        /// <summary>
        /// This URL exposes some data about current status as XML.
        /// </summary>
        private const string location = @"/cgi-bin/coutFile.cgi?fileName=%2Ftmp%2Fsystemdata.xml";

        static int Main(string[] args)
        {
            if (args.Length == 0)
                Console.Error.WriteLine("Usage: solarpoller <IP>");

            // Parse our IP address argument to ensure it is a valid IP. Handily prevents injections into the URL we
            // use later on.
            IPAddress addy;
            if (!IPAddress.TryParse(args[0], out addy))
                Console.Error.WriteLine("Bad IP address '" + args[0] + "'");

            // Try three times. If they all fail, print -1 and exit with a status code of -1.
            int retries = 3;
            while (retries > 0)
            {
                try
                {
                    WebRequest req = WebRequest.Create("http://" + addy.ToString() + location);

                    WebResponse resp = req.GetResponse();

                    string respString;
                    using (StreamReader respStream = new StreamReader(resp.GetResponseStream()))
                    {
                        respString = respStream.ReadToEnd();
                    }

                    // We don't even need to bother processing the XML fully. Just pull the number out
                    // of this tag.
                    string startToken = @"<PAC unit=""W"" class=""power"">";
                    string endToken = @"</PAC>";

                    int startPos = respString.IndexOf(startToken) + startToken.Length;
                    int endPos = respString.IndexOf(endToken);
                    string fishedOut = respString.Substring(startPos, endPos - startPos);

                    int num = int.Parse(fishedOut);

                    Console.WriteLine(num);
                    return 0;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Exception: " + e.Message);

                    retries--;
                }
            }

            Console.WriteLine("-1");
            return -1;
        }
    }
}
