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
    /**
     * Class for building certificate choice request and getting the response
     * <para>
     * Mandatory request parameters:
     * <ul>
     * <li><b>Host url</b> - can be set on the {@link ee.sk.smartid.SmartIdClient} level</li>
     * <li><b>Relying party uuid</b> - can either be set on the client or builder level</li>
     * <li><b>Relying party name</b> - can either be set on the client or builder level</li>
     * <li>Either <b>Document number</b> or <b>national identity</b></li>
     * </ul>
     * Optional request parameters:
     * <ul>
     * <li><b>Certificate level</b></li>
     * <li><b>Nonce</b></li>
     * </ul>
     */
    public class CertificateRequestBuilder : SmartIdRequestBuilder
    {
        /**
         * Constructs a new {@code CertificateRequestBuilder}
         *
         * @param connector for requesting certificate choice initiation
         * @param sessionStatusPoller for polling the certificate choice response
         */
        public CertificateRequestBuilder(ISmartIdConnector connector, SessionStatusPoller sessionStatusPoller)
            : base(connector, sessionStatusPoller)
        {
        }

        /**
         * Sets the request's UUID of the relying party
         * <para>
         * If not for explicit need, it is recommended to use
         * {@link ee.sk.smartid.SmartIdClient#setRelyingPartyUUID(String)}
         * instead. In that case when getting the builder from
         * {@link ee.sk.smartid.SmartIdClient} it is not required
         * to set the UUID every time when building a new request.
         *
         * @param relyingPartyUUID UUID of the relying party
         * @return this builder
         */
        public CertificateRequestBuilder WithRelyingPartyUUID(string relyingPartyUUID)
        {
            RelyingPartyUUID = relyingPartyUUID;
            return this;
        }

        /**
         * Sets the request's name of the relying party
         * <para>
         * If not for explicit need, it is recommended to use
         * {@link ee.sk.smartid.SmartIdClient#setRelyingPartyName(String)}
         * instead. In that case when getting the builder from
         * {@link ee.sk.smartid.SmartIdClient} it is not required
         * to set name every time when building a new request.
         *
         * @param relyingPartyName name of the relying party
         * @return this builder
         */
        public CertificateRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            RelyingPartyName = relyingPartyName;
            return this;
        }

        /**
         * Sets the request's document number
         * <para>
         * Document number is unique for the user's certificate/device
         * that is used for choosing the certificate.
         *
         * @param documentNumber document number of the certificate/device used to choose the certificate
         * @return this builder
         */
        public CertificateRequestBuilder WithDocumentNumber(string documentNumber)
        {
            DocumentNumber = documentNumber;
            return this;
        }

        /**
         * Sets the request's certificate level
         * <para>
         * Defines the minimum required level of the certificate.
         * Optional. When not set, it defaults to what is configured
         * on the server side i.e. "QUALIFIED".
         *
         * @param certificateLevel the level of the certificate
         * @return this builder
         */
        public CertificateRequestBuilder WithCertificateLevel(string certificateLevel)
        {
            CertificateLevel = certificateLevel;
            return this;
        }

        /**
         * Sets the request's nonce
         * <para>
         * By default the certificate choice's initiation request
         * has idempotent behaviour meaning when the request
         * is repeated inside a given time frame with exactly
         * the same parameters, session ID of an existing session
         * can be returned as a result. When requester wants, it can
         * override the idempotent behaviour inside of this time frame
         * using an optional "nonce" parameter present for all POST requests.
         * <para>
         * Normally, this parameter can be omitted.
         *
         * @param nonce nonce of the request
         * @return this builder
         */
        public CertificateRequestBuilder WithNonce(string nonce)
        {
            Nonce = nonce;
            return this;
        }

        /**
         * Specifies capabilities of the user
         * <para>
         * By default there are no specified capabilities.
         * The capabilities need to be specified in case of
         * a restricted Smart ID user
         * {@link #withCapabilities(String...)}
         * @param capabilities are specified capabilities for a restricted Smart ID user
         *                     and is one of [QUALIFIED, ADVANCED]
         * @return this builder
         */
        public CertificateRequestBuilder WithCapabilities(params Capability[] capabilities)
        {
            this.capabilities = new HashSet<string>(capabilities.Select(x => x.ToString()));
            return this;
        }

        /**
         * Specifies capabilities of the user
         * <para>
         *
         * By default there are no specified capabilities.
         * The capabilities need to be specified in case of
         * a restricted Smart ID user
         * {@link #withCapabilities(Capability...)}
         * @param capabilities are specified capabilities for a restricted Smart ID user
         *                     and is one of ["QUALIFIED", "ADVANCED"]
         * @return this builder
         */
        public CertificateRequestBuilder WithCapabilities(params string[] capabilities)
        {
            this.capabilities = new HashSet<string>(capabilities);
            return this;
        }

        /**
         * Sets the request's personal semantics identifier
         * <para>
         * Semantics identifier consists of identity type, country code, a hyphen and the identifier.
         *
         * @param semanticsIdentifierAsString semantics identifier for a person
         * @return this builder
         */
        public CertificateRequestBuilder WithSemanticsIdentifierAsString(string semanticsIdentifierAsString)
        {
            SemanticsIdentifier = new SemanticsIdentifier(semanticsIdentifierAsString);
            return this;
        }

        /**
         * Sets the request's personal semantics identifier
         * <para>
         * Semantics identifier consists of identity type, country code, and the identifier.
         *
         * @param semanticsIdentifier semantics identifier for a person
         * @return this builder
         */
        public CertificateRequestBuilder WithSemanticsIdentifier(SemanticsIdentifier semanticsIdentifier)
        {
            SemanticsIdentifier = semanticsIdentifier;
            return this;
        }

        /**
         * Send the certificate choice request and get the response
         *x
         /// <exception cref="UserAccountNotFoundException when the certificate was not found
         /// <exception cref="UserRefusedException when the user has refused the session.
         /// <exception cref="SessionTimeoutException when there was a timeout, i.e. end user did not confirm or refuse the operation within given timeframe
         /// <exception cref="DocumentUnusableException when for some reason, this relying party request cannot be completed.
         *                                   User must either check his/her Smart-ID mobile application or turn to customer support for getting the exact reason.
         /// <exception cref="ServerMaintenanceException when the server is under maintenance
         *
         * @return the certificate choice response
         */
        public async Task<SmartIdCertificate> FetchAsync(CancellationToken cancellationToken = default)
        {
            ValidateParameters();
            string sessionId = await InitiateCertificateChoiceAsync(cancellationToken);
            SessionStatus sessionStatus = await SessionStatusPoller.FetchFinalSessionStatusAsync(sessionId, cancellationToken);
            return CreateSmartIdCertificate(sessionStatus);
        }

        /**
         * Send the certificate choice request and get the session Id
         *
         /// <exception cref="UserAccountNotFoundException when the user account was not found
         /// <exception cref="ServerMaintenanceException when the server is under maintenance
         *
         * @return session Id - later to be used for manual session status polling
         */
        public async Task<string> InitiateCertificateChoiceAsync(CancellationToken cancellationToken = default)
        {
            ValidateParameters();
            CertificateRequest request = CreateCertificateRequest();
            CertificateChoiceResponse response = await FetchCertificateChoiceSessionResponseAsync(request, cancellationToken);
            return response.SessionId;
        }

        /**
         * Create {@link SmartIdCertificate} from {@link SessionStatus}
         * <para>
         * This method uses automatic session status polling internally
         * and therefore blocks the current thread until certificate choice is concluded/interupted etc.
         *
         /// <exception cref="UserRefusedException when the user has refused the session. NB! This exception has subclasses to determine the screen where user pressed cancel.
         /// <exception cref="SessionTimeoutException when there was a timeout, i.e. end user did not confirm or refuse the operation within given timeframe
         /// <exception cref="DocumentUnusableException when for some reason, this relying party request cannot be completed.
         *
         * @param sessionStatus session status response
         * @return the authentication response
         */
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
                return await Connector.GetCertificateAsync(SemanticsIdentifier, request, cancellationToken);
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
            if (certificate == null || string.IsNullOrWhiteSpace(certificate.Value))
            {
                throw new UnprocessableSmartIdResponseException("Certificate was not present in the session status response");
            }
            if (string.IsNullOrWhiteSpace(sessionStatus.Result.DocumentNumber))
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