using System;
using System.Collections.Generic;
using System.Text;

namespace FaceApp.Response
{
    public interface IFaceResponse
    {
        bool IsSuccess { get; }
        int StatusCode { get; set; }
    }
}
