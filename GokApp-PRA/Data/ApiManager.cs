using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GokApp_PRA.Data
{
    internal class ApiManager
    {
        public async Task<List<Match>> GetMatchAsync()
        {
            using (var client = new HttpClient())
            {
                string apiUrl = "http://c3-schoolvoetbal-php.test/api/matches/scores";

                HttpResponseMessage responseMessage = await client.GetAsync(apiUrl); ;
                responseMessage.EnsureSuccessStatusCode();

                string jsonResponse = await responseMessage.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<Match>>(jsonResponse);
            }
        }

        public class Match
        {
            [JsonProperty("team_1")]
            public string team_1 { get; set; }

            [JsonProperty("team_2")]
            public string team_2 { get; set; }

            [JsonProperty("team_1_score")]
            public string team_1_score { get; set; }

            [JsonProperty("team_2_score")]
            public string team_2_score { get; set; }
        }
    }
}
