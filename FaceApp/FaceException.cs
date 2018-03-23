using System;
using System.Collections.Generic;
using System.Text;

namespace FaceApp
{
    public class FaceException : Exception
    {
        public ExceptionType Type { get; set; }

        public FaceException(ExceptionType type, string message) : base(message)
        {
            this.Type = type;
        }

        public override string ToString()
        {
            return $"{Type}: {Message}";
        }
    }
}
