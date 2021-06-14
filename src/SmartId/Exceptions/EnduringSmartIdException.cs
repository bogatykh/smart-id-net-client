using System;

namespace SK.SmartId.Exceptions
{
    /// <summary>
    /// Exceptions that subclass this mark situations where something is wrong with
    /// client-side integration or how Relying Party account has been configured by Smart-ID operator
    /// or Smart-ID server is under maintenance.
    /// <para>With these types of errors there is not recommended to ask the user for immediate retry.</para>
    /// </summary>
    [Serializable]
    public abstract class EnduringSmartIdException : SmartIdException
    {
        protected EnduringSmartIdException(string message)
            : base(message)
        {
        }

        protected EnduringSmartIdException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}