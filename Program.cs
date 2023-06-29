﻿// See https://aka.ms/new-console-template for more information

// for c# http server: 
// main function to start the server
// function to handle requests
// page template to serve and data to draw on
// example https://gist.github.com/define-private-public/d05bc52dd0bed1c4699d49e2737e80e7
//  has these things in an HTTPServer class file.
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace HttpListenerExample
{
    class HttpServer 
    {
        public static HttpListener listener;
        public static string url = "http://localhost:8080/";
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static Random random = new Random();
        public static string pageTemplate =
        @"<!DOCTYPE>
            <html>
              <head>
                <title>HttpListener Example</title>
              </head>
              <body>
                <p>Coin Toss Result: {0}</p>
                <button>Toss a Coin</button>
                <p>Page Views: {1}</p>
                <form method=""post"" action=""shutdown"">
                  <input type=""submit"" value=""Shutdown"" {2}>
                </form>
              </body>
            </html>";
        public static string CoinToss()
        {
            
            int randNum = random.Next(0,2);
            Console.WriteLine("CoinToss() {0}", randNum);
            var result = randNum == 0 ? "heads" : "tails";
            return result;
        }
        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    Console.WriteLine("Shutdown requested");
                    runServer = false;
                }

                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                // Write the response info
                string disableSubmit = !runServer ? "disabled" : "";
                string coinTossResult = CoinToss();
                byte[] data = Encoding.UTF8.GetBytes(String.Format(pageTemplate, coinTossResult, pageViews, disableSubmit));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }
        public static void Main(string[] args)
        {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}
