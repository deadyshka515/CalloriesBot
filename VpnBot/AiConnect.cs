using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace CalloriesBot
{
    internal class AiConnect
    {
        // Важно! HttpClient должен быть один на всё приложение (или использовать IHttpClientFactory)
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:1234/")   // базовый адрес API
        };

        // POST запрос с JSON телом
        public static async Task<ModelResponse> Post(string additionUrl,ModelRequest request)
        {

            var response = await httpClient.PostAsJsonAsync(additionUrl, request);

            response.EnsureSuccessStatusCode();
            var answer = await response.Content.ReadFromJsonAsync<ModelResponse>();
            
            return answer;

        }
    }

}
