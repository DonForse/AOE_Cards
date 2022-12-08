using System;
using System.Runtime.Serialization;

namespace Features.Infrastructure.Services.Exceptions
{
    [Serializable]
    internal class MatchServiceException : Exception
    {
        public readonly string Error;
        public readonly long Code;

        public MatchServiceException()
        {
        }

        public MatchServiceException(string message) : base(message)
        {
        }

        public MatchServiceException(string error, long code)
        {
            this.Error = error;
            this.Code = code;
        }

        public MatchServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MatchServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}