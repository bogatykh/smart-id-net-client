using System;

namespace SK.SmartId.Exceptions.UserAccounts
{
    [Serializable]
    public class PersonShouldViewSmartIdPortalException : UserAccountException
    {
        public PersonShouldViewSmartIdPortalException()
            : base("Person should view Smart-ID app or Smart-ID self-service portal now.")
        {
        }

        public PersonShouldViewSmartIdPortalException(string message)
            : base(message)
        {
        }
    }
}