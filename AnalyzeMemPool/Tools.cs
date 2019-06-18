using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AnalyzeMemPool
{
    public static class Tools
    {
        public static string HttpPost(string Url, string postData, int timeOut = 60000)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    WebRequest request = WebRequest.Create(Url);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    request.ContentLength = byteArray.Length;
                    request.Timeout = timeOut;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    WebResponse response = request.GetResponse();
                    dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                    return responseFromServer;
                }
                catch (Exception we)
                {
                    Console.WriteLine(we.Message);
                    Console.WriteLine("Time out，retrying……");
                    continue;
                }
            }
            return "";
        }

        public static int GetMemPoolTransNum(string url)
        {
            int count = 0;

            try
            {
                var json = HttpPost(url, $@"
                {{
                    'jsonrpc': '2.0',
                    'method': 'getrawmempool',
                    'params': [],
                    'id': 1
                }}");
                JToken a = JObject.Parse(json)["result"];

                foreach (var tx in a.Values())
                {
                    count++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return count;
            
        }
    }
}
