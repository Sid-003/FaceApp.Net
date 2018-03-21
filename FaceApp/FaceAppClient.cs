﻿using Newtonsoft.Json;
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
        public async Task<Stream> ApplyFilterAsync(string code, FilterType filter)
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
                var exp = HandleException(errorCode);
                throw exp;
            }
            return await response.Content.ReadAsStreamAsync();           
        }

        /// <summary>
        /// Retrieves the image code from the image's url.
        /// </summary>
        /// <param name="uri"></param>
        /// The valid uri of the image.
        /// <returns></returns>
        public async Task<string> GetCodeAsync(Uri uri)
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
        public async Task<string> GetCodeAsync(string path)
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

        public FaceException HandleException(string errorCode)
        {
            switch (errorCode)
            {
                case "device_id_required":
                    return new FaceException(ExceptionType.NoDeviceIdFound, "No device id was found.");
                case "photo_no_file_content":
                    return new FaceException(ExceptionType.NoImageUploaded, "Image payload has an empty body.");
                case "photos_no_faces":
                    return new FaceException(ExceptionType.NoFacesDetected, "This image has no faces.");
                case "bad_filter_id":
                    return new FaceException(ExceptionType.BadFilter, "The filter specified was not valid");
                case "photo_not_found":
                    return new FaceException(ExceptionType.ImageNotFound, "No image found matching the provided image code.");
                default:
                    return new FaceException(ExceptionType.Unknown, "Unknown error occured."); 
            }
        }
        //Something only a madman would do. :^)
        private string GenerateDeviceId()
            => Guid.NewGuid().ToString().Replace("-", "").Substring(0, ID_LENGTH);
    }
}
