using System;

namespace SK.SmartId.Exceptions
{
    /// <summary>
    /// Subclasses of this exception are situation where something is wrong with user's Smart-ID account (or app) configuration.
    /// </summary>
    /// <remarks>General practise is to display a notification and ask user to log in to Smart-ID self-service portal.</remarks>
    [Serializable]
    public abstract class UserAccountException : SmartIdException
    {
        public UserAccountException(string s)
            : base(s)
        {
        }
    }
}