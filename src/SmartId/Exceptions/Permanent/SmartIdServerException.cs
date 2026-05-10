/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using System;

namespace SK.SmartId.Exceptions.Permanent
{
    /// <summary>
    /// Session status <c>endResult</c> was <c>SERVER_ERROR</c> (Java parity).
    /// </summary>
    [Serializable]
    public class SmartIdServerException : EnduringSmartIdException
    {
        public SmartIdServerException()
            : base("Process was terminated due to server-side technical error")
        {
        }
    }
}
