using System;

namespace SK.SmartId.Exceptions.UserActions
{
    [Serializable]
    public class CertificateLevelMismatchException : UserAccountException
    {
        public CertificateLevelMismatchException()
            : base("Signer's certificate is below requested certificate level")
        {
        }
    }
}