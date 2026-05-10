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
    /// Session status <c>endResult</c> was <c>EXPECTED_LINKED_SESSION</c> (Java parity).
    /// </summary>
    [Serializable]
    public class ExpectedLinkedSessionException : EnduringSmartIdException
    {
        public ExpectedLinkedSessionException()
            : base("The app received a different transaction while waiting for the linked session that follows the device-link based cert-choice session")
        {
        }
    }
}
