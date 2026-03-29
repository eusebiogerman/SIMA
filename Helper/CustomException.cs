using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Helper
{
    public class CustomException : Exception
    {
        private string? _message;
        private readonly Exception? _innerException;

        public override string Message { get => _message; }
        public override string? StackTrace => base.StackTrace;
        public  Exception? InnerException => _innerException;
        public CustomException()
        {
        }
        public CustomException(string? message) : base(message)
        {
            _message = message;
        }
        public CustomException(string? message, Exception? innerException) : base(message, innerException)
        {
            _innerException = innerException;

        }
        protected CustomException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
