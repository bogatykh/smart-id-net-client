/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>Maps an authentication certificate to <see cref="AuthenticationIdentity"/> (Java <c>AuthenticationIdentityMapper</c>).</summary>
    public static class AuthenticationIdentityMapper
    {
        public static AuthenticationIdentity From(X509Certificate2 certificate) =>
            AuthenticationResponseValidator.ConstructAuthenticationIdentity(certificate);
    }
}
