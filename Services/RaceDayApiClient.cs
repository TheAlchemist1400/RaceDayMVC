using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Mvc.Services
{
    //  Communication bridge between our MVC controllers and the REST API.
    /// HttpClientFactory: creates the configured HTTP client that knows your API's base URL.
    /// Session cookie forwarding: retrieves the API's session cookie from MVC session and sends it with subsequent API requests.
    /// JSON serialization: converts C# objects into JSON requests and API JSON responses back into C# objects.


    public class RaceDayApiClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
        {
            _factory = factory;
            _httpContextAccessor = accessor;
        }

        private HttpClient CreateClient()
        {
            return _factory.CreateClient("RaceDayApi");
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string url) 
        {
            var request = new HttpRequestMessage(method, url);

            string? apiCookie = _httpContextAccessor.HttpContext?.Session.GetString("ApiSessionCookie");

            if (!string.IsNullOrWhiteSpace(apiCookie))
            {
                request.Headers.TryAddWithoutValidation("Cookie", apiCookie);
            }

            return request;
        }

        public async Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string url,
            object? body = null)
        {
            var client = CreateClient();

            using var request = CreateRequest(method, url);

            if (body != null)
            {
                request.Content = JsonContent.Create(body);
            }

            return await client.SendAsync(request);
        }


        // Sends an image from MVC to the API using multipart/form-data.
        // Reuses the existing API session cookie.
        public async Task<HttpResponseMessage> UploadImageAsync(
            string url,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException(
                    "Please select an image to upload.");
            }

            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                throw new ArgumentException(
                    "Image must not exceed 5 MB.");
            }

            // STEP 1: Create the configured HTTP client.
            var client = CreateClient();

            // STEP 2: Create an authenticated POST request.
            using var request = CreateRequest(
                HttpMethod.Post,
                url
            );

            // STEP 3: Create multipart/form-data content.
            using var form = new MultipartFormDataContent();

            // STEP 4: Open the uploaded file stream.
            using var stream = file.OpenReadStream();

            using var fileContent = new StreamContent(stream);

            // STEP 5: Add the file to the form.
            // The field name "file" must match the API parameter.
            form.Add(
                fileContent,
                "file",
                file.FileName
            );

            // STEP 6: Attach multipart content to the request.
            request.Content = form;

            // STEP 7: Forward the request to the API.
            return await client.SendAsync(request);
        }


        public async Task<T?> ReadAsync<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            string json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        public static string? ExtractSessionCookie(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues(
                "Set-Cookie", out var values))
            {
                return null;
            }

            string? sessionHeader = values.FirstOrDefault(v =>
                v.StartsWith(
                    ".AspNetCore.Session=",
                    StringComparison.OrdinalIgnoreCase));

            return sessionHeader?.Split(';', 2)[0];
        }
    }
}
