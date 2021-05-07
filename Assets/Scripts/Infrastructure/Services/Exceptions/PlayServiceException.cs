using System;

namespace Infrastructure.Services
{
    internal class PlayServiceException : Exception
    {
        public readonly long Code;
        public readonly string Error;

        public PlayServiceException()
        {
        }

        public PlayServiceException(long code, string error)
        {
            this.Code = code;
            this.Error = error;
        }
    }
}