using System;

namespace FaceApp
{
    public class FaceException : Exception
    {
        public ExceptionType Type { get; }

<<<<<<< master
        public FaceException(ExceptionType type, string message) : base(message) 
            => Type = type;

        public override string ToString() 
=======
        public FaceException(ExceptionType type, string message) : base(message)
            => Type = type;
        

        public override string ToString()
>>>>>>> master
            => $"{Type}: {Message}";
    }
}
