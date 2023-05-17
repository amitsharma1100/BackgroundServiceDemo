using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using BackgroundServiceDemo.Models;
using Newtonsoft.Json;

namespace BackgroundServiceDemo.Web
{
    public class RestService : IRestService
    {
        private static Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(CreateClient, isThreadSafe: true);
        public static Uri StocksBaseUri { get; set; }
        public static Uri ExchangeBaseUri { get; set; }
        private static string ApiToken { get; set; }
        private static string AppId { get; set; }

        static RestService()
        {
            StocksBaseUri = new Uri("https://eodhistoricaldata.com/api/real-time/");
            ExchangeBaseUri = new Uri("https://openexchangerates.org/api/latest.json");
            ApiToken = "64649b76bd7c70.73633735";
            AppId = "8f5e98ca2c094d00a5414f64693e0042";
        }

        private static HttpClient CreateClient()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            HttpClient httpClient = new HttpClient();

            httpClient.Timeout = new TimeSpan(0, 0, 120);

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }

        internal HttpClient HttpClient
        {
            get
            {
                if (!_httpClient.IsValueCreated)
                    InitializeHttpClient();

                return _httpClient.Value;
            }
        }

        private void InitializeHttpClient()
        {
            if (StocksBaseUri == null)
                throw new ArgumentNullException("BaseUri");

            _httpClient.Value.BaseAddress = StocksBaseUri;
        }

        public async Task<string> GetRealTimeStockPriceByName(string name, string exchange)
        {
            try
            {
                var response = await HttpClient.GetAsync($"{StocksBaseUri.AbsoluteUri}{name}.{exchange}?fmt=json&api_token={ApiToken}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return await GetResponseAsync(response);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public async Task<ExchangeRatesResponse> GetRealTimeExchangeRates()
        {
            try
            {
                var uri = $"{ExchangeBaseUri.AbsoluteUri}?app_id={AppId}";
                var response = await HttpClient.GetAsync(uri);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return await GetResponseAsObject<ExchangeRatesResponse> (response);
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static async Task<T> GetResponseAsObject<T>(HttpResponseMessage response, bool asStream = false)
        {
            if (asStream)
                return await GetResponseAsStreamAsync<T>(response);

            var responseString = await GetResponseAsync(response);
            return JsonConvert.DeserializeObject<T>(responseString);
        }

        private static async Task<T> GetResponseAsStreamAsync<T>(HttpResponseMessage response)
        {
            using (Stream stream = await response.Content.ReadAsStreamAsync())
            using (var sr = new StreamReader(stream))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var serializer = new Newtonsoft.Json.JsonSerializer();

                // read the json from a stream
                // json size doesn't matter because only a small piece is read at a time from the HTTP request
                return serializer.Deserialize<T>(reader);
            }
        }

        private static async Task<string> GetResponseAsync(HttpResponseMessage response)
        {
            string responseContent = "";
            if (response.Content.Headers.ContentEncoding.FirstOrDefault() == "gzip")
            {
                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                using (Stream stream = new GZipStream(responseStream, CompressionMode.Decompress))
                using (var sr = new StreamReader(stream))
                {
                    responseContent = await sr.ReadToEndAsync();
                }
            }
            else
            {
                responseContent = await response.Content.ReadAsStringAsync();
            }

            return responseContent;
        }
    }
}
