using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FaceApp.Response
{
    public class FaceApplyResponse : IFaceResponse
    {
        public bool IsSuccess => true;
        public int StatusCode { get; set; }
        public Stream ImageStream { get; set; }
    }
}
