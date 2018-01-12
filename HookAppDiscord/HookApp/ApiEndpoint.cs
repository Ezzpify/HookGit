using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using HookAppDiscord.HookApp.DataHolders;

namespace HookAppDiscord.HookApp
{
    class ApiEndpoint
    {
        public static string ServerStatsUrl { get; set; }

        public static ServerStats GetServerStats()
        {
            if (string.IsNullOrWhiteSpace(ServerStatsUrl))
                throw new Exception("ServerStatsUrl is empty. Needs to be set before calling this function.");

            var client = new RestClient(ServerStatsUrl);
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<ServerStats>(Utils.FixJsonString(response.Content));
        }
    }
}
