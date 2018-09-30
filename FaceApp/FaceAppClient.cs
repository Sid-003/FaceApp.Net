using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FaceApp
{
    public class FaceAppClient
    {
        private const string BaseUrl = "https://node-01.faceapp.io/api/v2.9/photos";
        private const string UserAgent = "FaceApp/1.0.229 (Linux; Android 4.4)";
        private const int IdLength = 8;

        private string _deviceId;
        private HttpClient _client;

        private readonly ImmutableArray<FilterType> ProFilters = ImmutableArray.Create(

            FilterType.Bangs,
            FilterType.Female,
            FilterType.Female_2,
            FilterType.Glasses,
            FilterType.Goatee,
            FilterType.Heisenberg,
            FilterType.Hipster,
            FilterType.Hitman,
            FilterType.Hollywood,
            FilterType.Impression,
            FilterType.Lion,
            FilterType.Makeup,
            FilterType.Male,
            FilterType.Mustache,
            FilterType.Pan,
            FilterType.Wave
        );

        public FaceAppClient(HttpClient client)
            => (_client, _deviceId) = (client, GenerateDeviceId());
        

 
        /// <summary>
        /// Applies the filter type provided using the image code.
        /// </summary>
        /// <param name="code"></param>
        /// Image code provided by the API.
        /// <param name="filter"></param>
        /// Type of filter to be applied.
        /// <returns></returns>
        public async Task<Stream> ApplyFilterAsync(string code, FilterType filter, CancellationToken ct = default(CancellationToken))
        {
            bool cropped = false;
            if (ProFilters.Any(x => x == filter))
                cropped = true;
            var reqUrl = $"{BaseUrl}/{code}/filters/{filter.ToString().ToLower()}?cropped={cropped}";
            var request = new HttpRequestMessage(HttpMethod.Get, reqUrl);
            request.Headers.Add("User-Agent", UserAgent);
            request.Headers.Add("X-FaceApp-DeviceID", _deviceId);
            ct.ThrowIfCancellationRequested();
            var response = await _client.SendAsync(request, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                string errorCode = null;
                if (response.Headers.TryGetValues("X-FaceApp-ErrorCode", out var codes))
                    errorCode = codes.First();
                var exp = HandleException(errorCode);
                throw exp;
            }
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves the image code from the image's url.
        /// </summary>
        /// <param name="uri"></param>
        /// The valid uri of the image.
        /// <returns></returns>
        public async Task<string> GetCodeAsync(Uri uri, CancellationToken ct = default(CancellationToken))
        {
            using (var imageStream = await _client.GetStreamAsync(uri).ConfigureAwait(false))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
                request.Headers.Add("User-Agent", UserAgent);
                request.Headers.Add("X-FaceApp-DeviceID", _deviceId);
                var streamContent = new StreamContent(imageStream);
                var mutipartContent = new MultipartFormDataContent
                {
                    { streamContent, "file", "file" }
                };
                request.Content = mutipartContent;
                ct.ThrowIfCancellationRequested();
                var response = await _client.SendAsync(request, ct).ConfigureAwait(false);
                var jsonStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    string errorCode = null;
                    if (response.Headers.TryGetValues("X-FaceApp-ErrorCode", out var codes))
                        errorCode = codes.First();
                    var exp = HandleException(errorCode);
                    throw exp;
                }
                return JObject.Parse(jsonStr)["code"].ToString();
            }
        }

        /// <summary>
        /// Retrieves the image code from the image's path.
        /// </summary>
        /// <param name="path"></param>
        /// Valid path of the file.
        /// <returns></returns>
        public async Task<string> GetCodeAsync(string path, CancellationToken ct = default(CancellationToken))
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The file specified was not found.");
            using (var imageStream = File.Open(path, FileMode.Open))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
                request.Headers.Add("User-Agent", UserAgent);
                request.Headers.Add("X-FaceApp-DeviceID", _deviceId);
                var fileName = Path.GetFileName(path);
                var streamContent = new StreamContent(imageStream);
                var mutipartContent = new MultipartFormDataContent
                {
                    { streamContent, "file", fileName }
                };
                request.Content = mutipartContent;
                ct.ThrowIfCancellationRequested();
                var response = await _client.SendAsync(request, ct).ConfigureAwait(false);
                var jsonStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    string errorCode = null;
                    if (response.Headers.TryGetValues("X-FaceApp-ErrorCode", out var codes))
                        errorCode = codes.First();
                    var exp = HandleException(errorCode);
                    throw exp;
                }
                return JObject.Parse(jsonStr)["code"].ToString();
            }
        }

        private FaceException HandleException(string errorCode)
        {
            switch (errorCode)
            {
                case "device_id_required":
                    return new FaceException(ExceptionType.NoDeviceIdFound, "DeviceId not provided.");
                case "photo_no_file_content":
                    return new FaceException(ExceptionType.NoImageUploaded, "Empty image payload provided.");
                case "photos_no_faces":
                    return new FaceException(ExceptionType.NoFacesDetected, "No faces detected for the provided image.");
                case "bad_filter_id":
                    return new FaceException(ExceptionType.BadFilter, "Invalid filter provided.");
                case "photo_not_found":
                    return new FaceException(ExceptionType.ImageNotFound, "Image not found with the provided image code.");
                default:
                    return new FaceException(ExceptionType.Unknown, "Unknown Error.");
            }
        }

        //Something only a madman would do. :^)
        private string GenerateDeviceId()
            => Guid.NewGuid().ToString().Replace("-", "").Substring(0, IdLength);
    }
}
