using System;

namespace SK.SmartId.Exceptions.UserActions
{
    [Serializable]
    public class NoSuitableAccountOfRequestedTypeFoundException : UserAccountException
    {
        public NoSuitableAccountOfRequestedTypeFoundException()
            : base("No suitable account of requested type found, but user has some other accounts.")
        {
        }
    }
}
