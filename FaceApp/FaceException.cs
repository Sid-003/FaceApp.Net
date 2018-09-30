using System;
using System.Collections.Generic;
using System.Text;

namespace FaceApp
{
    public class FaceException : Exception
    {
        public ExceptionType Type { get; }

        public FaceException(ExceptionType type, string message) : base(message)
            => Type = type;
        

        public override string ToString()
            => $"{Type}: {Message}";
    }
}
