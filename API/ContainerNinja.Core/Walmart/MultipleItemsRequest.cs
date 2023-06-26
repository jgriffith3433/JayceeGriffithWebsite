using System.Collections.Specialized;
using System.Text.Json;

namespace ContainerNinja.Core.Walmart
{
    public class MultipleItemsRequest
    {
        public string ids { get; set; }

        public async Task<T> GetResponse<T>()
        {
            using (var client = new WalmartWebClient())
            {
                var t1 = client.GetTimestampInJavaMillis();
                var requiredHeaders = new NameValueCollection
                {
                    { "WM_CONSUMER.ID", "b8159879-e6cc-459a-888d-a47da6d224a3" },
                    { "WM_CONSUMER.INTIMESTAMP", t1 },
                    { "WM_SEC.KEY_VERSION", "3" },
                };
                client.Headers.Add(requiredHeaders);
                client.Headers.Add("WM_SEC.AUTH_SIGNATURE", client.GetWalmartSignature(requiredHeaders[0], requiredHeaders[1], requiredHeaders[2]));

                client.BaseAddress = "https://developer.api.walmart.com";
                var jsonResponse = await client.DownloadStringTaskAsync(string.Format("/api-proxy/service/affil/product/v2/items?ids{0}", ids));
                return JsonSerializer.Deserialize<T>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    AllowTrailingCommas = true,
                });
            }
        }
    }
}
