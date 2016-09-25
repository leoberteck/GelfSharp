using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GelfSharpCore.src
{
    public static class GelfShrapClient
    {
        public static string address { get; private set; }
        public static string serverPath { get; private set; }
        public static bool isRunning  { get; private set; }
        public static long sentMessages { get; set; }
        private static HttpClient httpClient { get; set; } 

        public static void Init(string _address, string _serverPath)
        {
            sentMessages = 0;
            address = _address;
            serverPath = _serverPath;
            httpClient = new HttpClient();
            isRunning = true;
        }

        public static async Task<HttpResponseMessage> SendMessageAsync(GelfMessage message)
        {
            if (isRunning)
            {
                sentMessages++;
                httpClient.BaseAddress = new Uri(address);
                var json = await message.SerializeToJsonAsync();
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                return await httpClient.PostAsync(serverPath, content);
            }
            else
            {
                throw new InvalidOperationException("Gelf client is not running");
            }
        }
    }
}
