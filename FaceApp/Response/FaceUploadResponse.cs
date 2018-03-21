using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FaceApp.Response
{
    public class FaceUploadResponse : IFaceResponse
    {
        public bool IsSuccess => true;
        public int StatusCode { get; set; }
        public string ImageCode { get; set; }
    }
}
