using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Helper
{
    public enum ExceptionType
    {
        DBErrorFailed = 1,
        SystemErrorFailed = 2,
        CustomError = 3,
        None = 4
    }

    public class CustomException : Exception
    {

        public static class ExcetionMessage {

           public static string DBErrorFailed { get => "DataBase Error Failed"; }
           public static string SystemErrorFailed { get => "System Error Failed"; }
           public static string CustomErrorFailed { get => "Window Error "; }
        }

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
