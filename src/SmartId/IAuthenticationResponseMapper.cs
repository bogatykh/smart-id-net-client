/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Rest.Dao;

namespace SK.SmartId
{
    /// <summary>Maps <see cref="SessionStatus"/> to <see cref="AuthenticationResponse"/> (Java <c>AuthenticationResponseMapper</c>).</summary>
    public interface IAuthenticationResponseMapper
    {
        AuthenticationResponse From(SessionStatus sessionStatus);
    }
}
