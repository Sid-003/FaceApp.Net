using FaceApp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FaceApp
{
    public class FaceAppClient
    {
        private const string BASE_URL = "https://node-01.faceapp.io/api/v2.3/photos";
        private const string USER_AGENT = "FaceApp/1.0.229 (Linux; Android 4.4)";
        private const int ID_LENGTH = 8;
        private string _deviceId;
        private HttpClient _client; 

        public FaceAppClient(HttpClient client)
        {
            _client = client;
            _deviceId = GenerateDeviceId();
        }

        
        /// <summary>
        /// Applies the filter type provided using the image code.
        /// </summary>
        /// <param name="code"></param>
        /// Image code provided by the API.
        /// <param name="filter"></param>
        /// Type of filter to be applied.
        /// <returns></returns>
        public async Task<IFaceResponse> ApplyFilterAsync(string code, FilterType filter)
        {
            bool cropped = false;
            if (filter == FilterType.Male || filter == FilterType.Female)
                cropped = true;
            var reqUrl = $"{BASE_URL}/{code}/filters/{Enum.GetName(typeof(FilterType), filter).ToLower()}?cropped={cropped}";
            var request = new HttpRequestMessage(HttpMethod.Get, reqUrl);
            request.Headers.Add("User-Agent", USER_AGENT);
            request.Headers.Add("X-FaceApp-DeviceID", _deviceId);
            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string errorCode = null;
                if (response.Headers.TryGetValues("X-FaceApp-ErrorCode", out var codes))
                    errorCode = codes.First();
                var errorResponse = new FaceErrorResponse
                {
                    Description = null,
                    ErrorCode = errorCode,
                    StatusCode = (int)response.StatusCode
                };
                return errorResponse;
            }
            var imgStream = await response.Content.ReadAsStreamAsync();
            var uploadResponse = new FaceApplyResponse
            {
                ImageStream = imgStream,
                StatusCode = (int)response.StatusCode
            };
            return uploadResponse;
        }

        /// <summary>
        /// Retrieves the image code from the image's url.
        /// </summary>
        /// <param name="uri"></param>
        /// The valid uri of the image.
        /// <returns></returns>
        public async Task<IFaceResponse> GetCodeAsync(Uri uri)
        {
            using (var imageStream = await _client.GetStreamAsync(uri))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, BASE_URL);
                request.Headers.Add("User-Agent", USER_AGENT);
                request.Headers.Add("X-FaceApp-DeviceID", _deviceId);
                var streamContent = new StreamContent(imageStream);
                var mutipartContent = new MultipartFormDataContent();
                mutipartContent.Add(streamContent, "file", "file");
                request.Content = mutipartContent;
                var response = await _client.SendAsync(request);
                var jsonStr = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var code = JObject.Parse(jsonStr)["code"].ToString(); ;
                    var faceResponse = new FaceUploadResponse
                    {
                        ImageCode = code,
                        StatusCode = (int)response.StatusCode
                    };
                    return faceResponse;
                }
                var faceErrorResponse = JsonConvert.DeserializeObject<FaceErrorResponse>(jsonStr);
                faceErrorResponse.StatusCode = (int)response.StatusCode;
                return faceErrorResponse;
            }
        }

        /// <summary>
        /// Retrieves the image code from the image's path.
        /// </summary>
        /// <param name="path"></param>
        /// Valid path of the file.
        /// <returns></returns>
        public async Task<IFaceResponse> GetCodeAsync(string path)
        {
            //too lazy to make a proper exception handler.
            if (!File.Exists(path))
                throw new FileNotFoundException("The file specified was not found.");
            using (var imageStream = File.Open(path, FileMode.Open))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, BASE_URL);
                request.Headers.Add("User-Agent", USER_AGENT);
                request.Headers.Add("X-FaceApp-DeviceID", _deviceId);
                var fileName = Path.GetFileName(path);
                var streamContent = new StreamContent(imageStream);
                var mutipartContent = new MultipartFormDataContent();
                mutipartContent.Add(streamContent, "file", fileName);
                request.Content = mutipartContent;
                var response = await _client.SendAsync(request);
                var jsonStr = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var code = JObject.Parse(jsonStr)["code"].ToString(); ;
                    var faceResponse = new FaceUploadResponse
                    {
                        ImageCode = code,
                        StatusCode = (int)response.StatusCode
                    };
                    return faceResponse;
                }
                var faceErrorResponse = JsonConvert.DeserializeObject<FaceErrorResponse>(jsonStr);
                faceErrorResponse.StatusCode = (int)response.StatusCode;
                return faceErrorResponse;

            }
        }


        //Something only a madman would do. :^)
        private string GenerateDeviceId()
            => Guid.NewGuid().ToString().Replace("-", "").Substring(0, ID_LENGTH);
    }
}
