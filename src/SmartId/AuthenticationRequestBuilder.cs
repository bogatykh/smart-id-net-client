/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 SK ID Solutions AS
 * %%
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId
{
    /// <summary>
    /// Class for building authentication request and getting the response
    /// <para>Mandatory request parameters:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <term>Host url</term>
    ///         <description>can be set on the {@link ee.sk.smartid.SmartIdClient} level</description>
    ///     </item>
    ///     <item>
    ///         <term>Relying party uuid</term>
    ///         <description>can either be set on the client or builder level</description>
    ///     </item>
    ///     <item>
    ///         <term>Relying party name</term>
    ///         <description>can either be set on the client or builder level</description>
    ///     </item>
    ///     <item>
    ///         <description>Either <b>Document number</b> or <b>semantics identifier</b> or <b>private company identifier</b></description>
    ///     </item>
    ///     <item>
    ///         <description><b>Authentication hash</b></description>
    ///     </item>
    /// </list>
    /// <para>Optional request parameters:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <description><b>Certificate level</b></description>
    ///     </item>
    ///     <item>
    ///         <description><b>Display text</b></description>
    ///     </item>
    ///     <item>
    ///         <description><b>Nonce</b></description>
    ///     </item>
    /// </list>
    /// </summary>
    public class AuthenticationRequestBuilder : SmartIdRequestBuilder
    {
        /// <summary>
        /// Constructs a new <see cref="AuthenticationRequestBuilder"/>
        /// </summary>
        /// <param name="connector">for requesting authentication initiation</param>
        /// <param name="sessionStatusPoller">for polling the authentication response</param>
        public AuthenticationRequestBuilder(ISmartIdConnector connector, SessionStatusPoller sessionStatusPoller)
            : base(connector, sessionStatusPoller)
        {
        }

        /// <summary>
        /// Sets the request's UUID of the relying party
        /// <para>
        /// If not for explicit need, it is recommended to use
        /// <see cref="SmartIdClient.RelyingPartyUUID"/>
        /// instead. In that case when getting the builder from
        /// <see cref="SmartIdClient"/> it is not required
        /// to set the UUID every time when building a new request.
        /// </para>
        /// </summary>
        /// <param name="relyingPartyUUID">UUID of the relying party</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithRelyingPartyUUID(string relyingPartyUUID)
        {
            RelyingPartyUUID = relyingPartyUUID;
            return this;
        }

        /// <summary>
        /// Sets the request's name of the relying party
        /// <para>
        /// If not for explicit need, it is recommended to use
        /// <see cref="SmartIdClient.RelyingPartyName"/>
        /// instead. In that case when getting the builder from
        /// <see cref="SmartIdClient"/>
        /// it is not required
        /// to set name every time when building a new request.
        /// </para>
        /// </summary>
        /// <param name="relyingPartyName">name of the relying party</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            RelyingPartyName = relyingPartyName;
            return this;
        }

        /// <summary>
        /// Sets the request's document number
        /// <para>
        /// Document number is unique for the user's certificate/device
        /// that is used for the authentication.
        /// </para>
        /// </summary>
        /// <param name="documentNumber">document number of the certificate/device to be authenticated</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithDocumentNumber(string documentNumber)
        {
            DocumentNumber = documentNumber;
            return this;
        }

        /// <summary>
        /// Sets the request's personal semantics identifier
        /// <para>
        /// Semantics identifier consists of identity type, country code, a hyphen and the identifier.
        /// </para>
        /// </summary>
        /// <param name="semanticsIdentifier">semantics identifier for a person</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithSemanticsIdentifierAsString(string semanticsIdentifier)
        {
            SemanticsIdentifier = new SemanticsIdentifier(semanticsIdentifier);
            return this;
        }

        /// <summary>
        /// Sets the request's personal semantics identifier
        /// <para>
        /// Semantics identifier consists of identity type, country code, and the identifier.
        /// </para>
        /// </summary>
        /// <param name="semanticsIdentifier">semantics identifier for a person</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithSemanticsIdentifier(SemanticsIdentifier semanticsIdentifier)
        {
            SemanticsIdentifier = semanticsIdentifier;
            return this;
        }

        /// <summary>
        /// Sets the request's authentication hash
        /// <para>
        /// It is the hash that is signed by a person's device
        /// which is essential for the authentication verification.
        /// For security reasons the hash should be generated
        /// randomly for every new request.It is recommended to use:
        /// <see cref="AuthenticationHash.GenerateRandomHash"/>
        /// </para>
        /// </summary>
        /// <param name="authenticationHash">hash used to sign for authentication</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithAuthenticationHash(AuthenticationHash authenticationHash)
        {
            hashToSign = authenticationHash;
            return this;
        }

        /// <summary>
        /// Sets the request's certificate level
        /// <para>
        /// Defines the minimum required level of the certificate.
        /// Optional.When not set, it defaults to what is configured
        /// on the server side i.e. "QUALIFIED".
        /// </para>
        /// </summary>
        /// <param name="certificateLevel">the level of the certificate</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithCertificateLevel(string certificateLevel)
        {
            CertificateLevel = certificateLevel;
            return this;
        }

        /// <summary>
        /// Sets the request's nonce
        /// <para>
        /// By default the authentication's initiation request
        /// has idempotent behaviour meaning when the request
        /// is repeated inside a given time frame with exactly
        /// the same parameters, session ID of an existing session
        /// can be returned as a result.When requester wants, it can
        /// override the idempotent behaviour inside of this time frame
        /// using an optional "nonce" parameter present for all POST requests.
        /// </para>
        /// <para>
        /// Normally, this parameter can be omitted.
        /// </para>
        /// </summary>
        /// <param name="nonce">nonce of the request</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithNonce(string nonce)
        {
            Nonce = nonce;
            return this;
        }

        /// <summary>
        /// Specifies capabilities of the user
        /// <para>
        /// By default there are no specified capabilities.
        /// The capabilities need to be specified in case of
        /// a restricted Smart ID user
        /// <see cref="WithCapabilities(string[])"/>
        /// </para>
        /// </summary>
        /// <param name="capabilities">are specified capabilities for a restricted Smart ID user and is one of [QUALIFIED, ADVANCED]</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithCapabilities(params Capability[] capabilities)
        {
            this.capabilities = new HashSet<string>(capabilities.Select(x => x.ToString()));
            return this;
        }

        /// <summary>
        /// Specifies capabilities of the user
        /// <para>
        /// By default there are no specified capabilities.
        /// The capabilities need to be specified in case of
        /// a restricted Smart ID user
        /// <see cref="WithCapabilities(Capability[])"/>
        /// </summary>
        /// <param name="capabilities">are specified capabilities for a restricted Smart ID user and is one of ["QUALIFIED", "ADVANCED"]</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithCapabilities(params string[] capabilities)
        {
            this.capabilities = new HashSet<string>(capabilities);
            return this;
        }

        /// <param name="allowedInteractionsOrder">Preferred order of what dialog to present to user. What actually gets displayed depends on user's device and its software version. First option from this list that the device is capable of handling is displayed to the user.</param>
        /// <returns>this builder</returns>
        public AuthenticationRequestBuilder WithAllowedInteractionsOrder(List<Interaction> allowedInteractionsOrder)
        {
            this.allowedInteractionsOrder = allowedInteractionsOrder;
            return this;
        }

        /// <summary>
        /// Send the authentication request and get the response
        /// <para>
        /// This method uses automatic session status polling internally
        /// and therefore blocks the current thread until authentication is concluded/interrupted etc.
        /// </para>
        /// </summary>
        /// <exception cref="Exceptions.UserActions.UserAccountNotFoundException">when the user account was not found</exception>
        /// <exception cref="Exceptions.UserActions.UserRefusedException">when the user has refused the session. NB! This exception has subclasses to determine the screen where user pressed cancel.</exception>
        /// <exception cref="Exceptions.UserActions.UserSelectedWrongVerificationCodeException">when user was presented with three control codes and user selected wrong code</exception>
        /// <exception cref="Exceptions.UserActions.SessionTimeoutException">when there was a timeout, i.e. end user did not confirm or refuse the operation within given timeframe</exception>
        /// <exception cref="Exceptions.UserAccounts.DocumentUnusableException">when for some reason, this relying party request cannot be completed. User must either check his/her Smart-ID mobile application or turn to customer support for getting the exact reason.</exception>
        /// <exception cref="Exceptions.Permanent.ServerMaintenanceException">when the server is under maintenance</exception>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the authentication response</returns>
        public async Task<SmartIdAuthenticationResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            string sessionId = await InitiateAuthenticationAsync(cancellationToken);
            SessionStatus sessionStatus = await SessionStatusPoller.FetchFinalSessionStatusAsync(sessionId, cancellationToken);
            return CreateSmartIdAuthenticationResponse(sessionStatus);
        }

        /// <summary>
        /// Send the authentication request and get the session Id
        /// </summary>
        /// <exception cref="Exceptions.UserActions.UserAccountNotFoundException">when the user account was not found</exception>
        /// <exception cref="Exceptions.Permanent.ServerMaintenanceException">when the server is under maintenance</exception>
        /// <param name="cancellationToken"></param>
        /// <returns>session Id - later to be used for manual session status polling</returns>
        public async Task<string> InitiateAuthenticationAsync(CancellationToken cancellationToken = default)
        {
            ValidateParameters();
            AuthenticationSessionRequest request = CreateAuthenticationSessionRequest();
            AuthenticationSessionResponse response = await GetAuthenticationResponseAsync(request, cancellationToken);
            return response.SessionId;
        }

        /// <summary>
        /// Create <see cref="SmartIdAuthenticationResponse"/> from <see cref="SessionStatus"/>
        /// </summary>
        /// <exception cref="Exceptions.UserActions.UserRefusedException">when the user has refused the session. NB! This exception has subclasses to determine the screen where user pressed cancel.</exception>
        /// <exception cref="Exceptions.UserActions.SessionTimeoutException">when there was a timeout, i.e. end user did not confirm or refuse the operation within given time frame</exception>
        /// <exception cref="Exceptions.UserActions.UserSelectedWrongVerificationCodeException">when user was presented with three control codes and user selected wrong code</exception>
        /// <exception cref="Exceptions.UserAccounts.DocumentUnusableException">when for some reason, this relying party request cannot be completed.</exception>
        /// <param name="sessionStatus">session status response</param>
        /// <returns>the authentication response</returns>
        public SmartIdAuthenticationResponse CreateSmartIdAuthenticationResponse(SessionStatus sessionStatus)
        {
            ValidateAuthenticationResponse(sessionStatus);

            SessionResult sessionResult = sessionStatus.Result;
            SessionSignature sessionSignature = sessionStatus.Signature;
            SessionCertificate certificate = sessionStatus.Cert;

            SmartIdAuthenticationResponse authenticationResponse = new SmartIdAuthenticationResponse()
            {
                EndResult = sessionResult.EndResult,
                SignedHashInBase64 = GetHashInBase64(),
                HashType = HashType,
                SignatureValueInBase64 = sessionSignature.Value,
                AlgorithmName = sessionSignature.Algorithm,
                Certificate = CertificateParser.ParseX509Certificate(certificate.Value),
                RequestedCertificateLevel = CertificateLevel,
                CertificateLevel = certificate.CertificateLevel,
                DocumentNumber = sessionResult.DocumentNumber,
                InteractionFlowUsed = sessionStatus.InteractionFlowUsed
            };

            return authenticationResponse;
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            ValidateAuthSignParameters();
        }

        private void ValidateAuthenticationResponse(SessionStatus sessionStatus)
        {
            ValidateSessionResult(sessionStatus.Result);
            if (sessionStatus.Signature == null)
            {
                throw new UnprocessableSmartIdResponseException("Signature was not present in the response");
            }
            if (sessionStatus.Cert == null)
            {
                throw new UnprocessableSmartIdResponseException("Certificate was not present in the response");
            }
        }

        private async Task<AuthenticationSessionResponse> GetAuthenticationResponseAsync(AuthenticationSessionRequest request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(DocumentNumber))
            {
                return await Connector.AuthenticateAsync(DocumentNumber, request, cancellationToken);
            }
            else
            {
                return await Connector.AuthenticateAsync(SemanticsIdentifier, request, cancellationToken);
            }
        }

        private AuthenticationSessionRequest CreateAuthenticationSessionRequest()
        {
            AuthenticationSessionRequest request = new AuthenticationSessionRequest()
            {
                RelyingPartyUUID = RelyingPartyUUID,
                RelyingPartyName = RelyingPartyName,
                CertificateLevel = CertificateLevel,
                HashType = GetHashTypeString(),
                Hash = GetHashInBase64(),
                Nonce = Nonce,
                Capabilities = Capabilities,
                AllowedInteractionsOrder = AllowedInteractionsOrder
            };
            return request;
        }
    }
}