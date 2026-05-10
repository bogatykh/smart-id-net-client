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
    /// Session status <c>endResult</c> was <c>PROTOCOL_FAILURE</c> (Java parity).
    /// </summary>
    [Serializable]
    public class ProtocolFailureException : EnduringSmartIdException
    {
        public ProtocolFailureException()
            : base("A logical error occurred in the signing protocol.")
        {
        }
    }
}
