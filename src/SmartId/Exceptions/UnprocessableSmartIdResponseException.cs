using SK.SmartId.Exceptions.Permanent;
using System;

namespace SK.SmartId.Exceptions
{
    [Serializable]
    public class UnprocessableSmartIdResponseException : SmartIdClientException
    {
        public UnprocessableSmartIdResponseException(string message)
            : base(message)
        {
        }

        public UnprocessableSmartIdResponseException(string s, Exception innerException)
            : base(s, innerException)
        {
        }
    }
}