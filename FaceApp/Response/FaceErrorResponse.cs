using FaceApp.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FaceApp
{
    public class FaceErrorResponse : IFaceResponse
    {
        [JsonIgnore]
        public bool IsSuccess => false;

        [JsonIgnore]
        public int StatusCode { get; set; }

        [JsonProperty("err")]
        public string ErrorCode { get; set; }

        [JsonProperty("desc")]
        public string Description { get; set; }

    }
}
