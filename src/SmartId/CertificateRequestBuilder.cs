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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId
{
    /// <summary>
    /// Class for building certificate choice request and getting the response
    /// <para>Mandatory request parameters:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <term>Host url</term>
    ///         <description>can be set on the <see cref="SmartIdClient"/> level</description>
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
    ///         <description><b>Nonce</b></description>
    ///     </item>
    /// </list>
    /// </summary>
    public class CertificateRequestBuilder : SmartIdRequestBuilder
    {
        /// <summary>
        /// Constructs a new <see cref="CertificateRequestBuilder"/>
        /// </summary>
        /// <param name="connector">for requesting certificate choice initiation</param>
        /// <param name="sessionStatusPoller">for polling the certificate choice response</param>
        public CertificateRequestBuilder(ISmartIdConnector connector, SessionStatusPoller sessionStatusPoller)
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
        public CertificateRequestBuilder WithRelyingPartyUUID(string relyingPartyUUID)
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
        /// <see cref="SmartIdClient"/> it is not required
        /// to set name every time when building a new request.
        /// </para>
        /// </summary>
        /// <param name="relyingPartyName">name of the relying party</param>
        /// <returns>this builder</returns>
        public CertificateRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            RelyingPartyName = relyingPartyName;
            return this;
        }

        /// <summary>
        /// Sets the request's document number
        /// <para>Document number is unique for the user's certificate/device that is used for choosing the certificate.</para>
        /// </summary>
        /// <param name="documentNumber">document number of the certificate/device used to choose the certificate</param>
        /// <returns>this builder</returns>
        public CertificateRequestBuilder WithDocumentNumber(string documentNumber)
        {
            DocumentNumber = documentNumber;
            return this;
        }

        /// <summary>
        /// Sets the request's certificate level
        /// <para>
        /// Defines the minimum required level of the certificate.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Optional. When not set, it defaults to what is configured
        /// on the server side i.e. "QUALIFIED".
        /// </remarks>
        /// <param name="certificateLevel">the level of the certificate</param>
        /// <returns>this builder</returns>
        public CertificateRequestBuilder WithCertificateLevel(string certificateLevel)
        {
            CertificateLevel = certificateLevel;
            return this;
        }

        /// <summary>
        /// Sets the request's nonce
        /// <para>
        /// By default the certificate choice's initiation request
        /// has idempotent behaviour meaning when the request
        /// is repeated inside a given time frame with exactly
        /// the same parameters, session ID of an existing session
        /// can be returned as a result. When requester wants, it can
        /// override the idempotent behaviour inside of this time frame
        /// using an optional "nonce" parameter present for all POST requests.
        /// </para>
        /// </summary>
        /// <remarks>Normally, this parameter can be omitted.</remarks>
        /// <param name="nonce">nonce of the request</param>
        /// <returns>this builder</returns>
        public CertificateRequestBuilder WithNonce(string nonce)
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
        public CertificateRequestBuilder WithCapabilities(params Capability[] capabilities)
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
        /// </para>
        /// </summary>
        /// <param name="capabilities">are specified capabilities for a restricted Smart ID user and is one of ["QUALIFIED", "ADVANCED"]</param>
        /// <returns>this builder</returns>
        public CertificateRequestBuilder WithCapabilities(params string[] capabilities)
        {
            this.capabilities = new HashSet<string>(capabilities);
            return this;
        }

        /// <summary>
        /// Sets the request's personal semantics identifier
        /// <para>Semantics identifier consists of identity type, country code, a hyphen and the identifier.</para>
        /// </summary>
        /// <param name="semanticsIdentifierAsString">semantics identifier for a person</param>
        /// <returns>this builder</returns>
        public CertificateRequestBuilder WithSemanticsIdentifierAsString(string semanticsIdentifierAsString)
        {
            SemanticsIdentifier = new SemanticsIdentifier(semanticsIdentifierAsString);
            return this;
        }

        /// <summary>
        /// Sets the request's personal semantics identifier
        /// <para>Semantics identifier consists of identity type, country code, and the identifier.</para>
        /// </summary>
        /// <param name="semanticsIdentifier">semantics identifier for a person</param>
        /// <returns>this builder</returns>
        public CertificateRequestBuilder WithSemanticsIdentifier(SemanticsIdentifier semanticsIdentifier)
        {
            SemanticsIdentifier = semanticsIdentifier;
            return this;
        }

        /// <summary>
        /// Send the certificate choice request and get the response
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the certificate choice response</returns>
        /// <exception cref="Exceptions.UserActions.UserAccountNotFoundException">when the certificate was not found</exception>
        /// <exception cref="Exceptions.UserActions.UserRefusedException">when the user has refused the session.</exception>
        /// <exception cref="Exceptions.UserActions.SessionTimeoutException">when there was a timeout, i.e. end user did not confirm or refuse the operation within given timeframe</exception>
        /// <exception cref="Exceptions.UserAccounts.DocumentUnusableException">when for some reason, this relying party request cannot be completed. User must either check his/her Smart-ID mobile application or turn to customer support for getting the exact reason.</exception>
        /// <exception cref="Exceptions.Permanent.ServerMaintenanceException">when the server is under maintenance</exception>
        public async Task<SmartIdCertificate> FetchAsync(CancellationToken cancellationToken = default)
        {
            ValidateParameters();
            string sessionId = await InitiateCertificateChoiceAsync(cancellationToken);
            SessionStatus sessionStatus = await SessionStatusPoller.FetchFinalSessionStatusAsync(sessionId, cancellationToken);
            return CreateSmartIdCertificate(sessionStatus);
        }

        /// <summary>
        /// Send the certificate choice request and get the session Id
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>session Id - later to be used for manual session status polling</returns>
        /// <exception cref="Exceptions.UserActions.UserAccountNotFoundException">when the user account was not found</exception>
        /// <exception cref="Exceptions.Permanent.ServerMaintenanceException">when the server is under maintenance</exception>
        public async Task<string> InitiateCertificateChoiceAsync(CancellationToken cancellationToken = default)
        {
            ValidateParameters();
            CertificateRequest request = CreateCertificateRequest();
            CertificateChoiceResponse response = await FetchCertificateChoiceSessionResponseAsync(request, cancellationToken);
            return response.SessionId;
        }

        /// <summary>
        /// Create <see cref="SmartIdCertificate"/> from <see cref="SessionStatus"/>
        /// </summary>
        /// <remarks>This method uses automatic session status polling internally and therefore blocks the current thread until certificate choice is concluded/interupted etc.</remarks>
        /// <param name="sessionStatus">session status response</param>
        /// <returns>the authentication response</returns>
        /// <exception cref="UserRefusedException when the user has refused the session. NB! This exception has subclasses to determine the screen where user pressed cancel.
        /// <exception cref="SessionTimeoutException when there was a timeout, i.e. end user did not confirm or refuse the operation within given timeframe
        /// <exception cref="DocumentUnusableException when for some reason, this relying party request cannot be completed.
        public SmartIdCertificate CreateSmartIdCertificate(SessionStatus sessionStatus)
        {
            ValidateCertificateResponse(sessionStatus);
            SessionCertificate certificate = sessionStatus.Cert;
            SmartIdCertificate smartIdCertificate = new SmartIdCertificate()
            {
                Certificate = CertificateParser.ParseX509Certificate(certificate.Value),
                CertificateLevel = certificate.CertificateLevel,
                DocumentNumber = GetDocumentNumber(sessionStatus)
            };
            return smartIdCertificate;
        }

        private async Task<CertificateChoiceResponse> FetchCertificateChoiceSessionResponseAsync(CertificateRequest request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(DocumentNumber))
            {
                return await Connector.GetCertificateAsync(DocumentNumber, request, cancellationToken);
            }
            else if (SemanticsIdentifier != null)
            {
                return await Connector.GetCertificateAsync(SemanticsIdentifier.Value, request, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Either set semanticsIdentifier or documentNumber");
            }
        }

        private CertificateRequest CreateCertificateRequest()
        {
            CertificateRequest request = new CertificateRequest()
            {
                RelyingPartyUUID = RelyingPartyUUID,
                RelyingPartyName = RelyingPartyName,
                CertificateLevel = CertificateLevel,
                Nonce = Nonce,
                Capabilities = Capabilities
            };
            return request;
        }

        public void ValidateCertificateResponse(SessionStatus sessionStatus)
        {
            ValidateSessionResult(sessionStatus.Result);
            SessionCertificate certificate = sessionStatus.Cert;
            if (certificate == null || string.IsNullOrEmpty(certificate.Value))
            {
                throw new UnprocessableSmartIdResponseException("Certificate was not present in the session status response");
            }
            if (string.IsNullOrEmpty(sessionStatus.Result.DocumentNumber))
            {
                throw new UnprocessableSmartIdResponseException("Document number was not present in the session status response");
            }
        }

        private string GetDocumentNumber(SessionStatus sessionStatus)
        {
            SessionResult sessionResult = sessionStatus.Result;
            return sessionResult.DocumentNumber;
        }
    }
}