using System;

namespace SK.SmartId.Exceptions
{
    /// <summary>
    /// Subclasses of this exception are situation where user's action triggered ending session.
    /// </summary>
    /// <remarks>General practise is to ask the user to try again.</remarks>
    [Serializable]
    public abstract class UserActionException : SmartIdException
    {
        public UserActionException(string s)
                : base(s)
        {
        }
    }
}