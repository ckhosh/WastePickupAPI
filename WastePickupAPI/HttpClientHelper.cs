using System.Text.Json;

namespace WastePickupAPI
{
    public static class HttpClientHelper
    {

        /// <summary>
        /// Seems to work fine. Could also just add JsonPropertyName to each property
        /// </summary>
        internal class LowerCaseNamingPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                return name.ToLower();
            }
        }

        /// <summary>
        /// Used to call and deserialize responses
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpClient"></param>
        /// <param name="addressURL"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<T?> GetAsync<T>(HttpClient httpClient, string addressURL)
        {
            var res = await httpClient.GetAsync(addressURL);
            if (res.IsSuccessStatusCode)
            {
                //Console.WriteLine(await res.Content.ReadAsStringAsync());
                var options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = new LowerCaseNamingPolicy(),
                };
                return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                string msg = await res.Content.ReadAsStringAsync();
                throw new Exception(msg);
            }
        }
    }
}
