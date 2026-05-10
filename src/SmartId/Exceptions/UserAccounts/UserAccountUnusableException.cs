/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using System;

namespace SK.SmartId.Exceptions.UserAccounts
{
    /// <summary>
    /// Session status <c>endResult</c> was <c>ACCOUNT_UNUSABLE</c> (Java parity).
    /// </summary>
    [Serializable]
    public class UserAccountUnusableException : UserAccountException
    {
        public UserAccountUnusableException()
            : base("The account is currently unusable")
        {
        }
    }
}
