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
    /// Class for building signature request and getting the response
    /// <para>
    /// Mandatory request parameters:
    /// </para>
    /// <list>
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
    ///     <item><b>Document number</b></item>
    ///     <item>Either<b> Signable hash</b> or <b>Signable data</b></item>
    /// </list>
    /// <para>
    /// Optional request parameters:
    /// </para>
    /// <list>
    ///     <item><b>Certificate level</b></item>
    ///     <item><b>Display text</b></item>
    ///     <item><b>Nonce</b></item>
    /// </list>
    /// </summary>
    public class SignatureRequestBuilder : SmartIdRequestBuilder
    {
        /// <summary>
        /// Constructs a new <see cref="SignatureRequestBuilder"/>
        /// </summary>
        /// <param name="connector">for requesting signing initiation</param>
        /// <param name="sessionStatusPoller">for polling the signature response</param>
        public SignatureRequestBuilder(ISmartIdConnector connector, SessionStatusPoller sessionStatusPoller)
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
        public SignatureRequestBuilder WithRelyingPartyUUID(string relyingPartyUUID)
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
        public SignatureRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            RelyingPartyName = relyingPartyName;
            return this;
        }

        /**
         * Sets the request's document number
         * <para>
         * Document number is unique for the user's certificate/device
         * that is used for the signing.
         *
         * @param documentNumber document number of the certificate/device used to sign
         * @return this builder
         */
        public SignatureRequestBuilder WithDocumentNumber(string documentNumber)
        {
            DocumentNumber = documentNumber;
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
        public SignatureRequestBuilder WithSemanticsIdentifierAsString(string semanticsIdentifierAsString)
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
        public SignatureRequestBuilder WithSemanticsIdentifier(SemanticsIdentifier semanticsIdentifier)
        {
            SemanticsIdentifier = semanticsIdentifier;
            return this;
        }

        /**
         * Sets the data of the document to be signed
         * <para>
         * This method could be used when the data
         * to be signed is not in hashed format.
         * {@link ee.sk.smartid.SignableData#setHashType(HashType)}
         * can be used to select the wanted hash type
         * and the data is hashed for you.
         *
         * @param dataToSign dat to be signed
         * @return this builder
         */
        public SignatureRequestBuilder WithSignableData(SignableData dataToSign)
        {
            base.dataToSign = dataToSign;
            return this;
        }

        /**
         * Sets the hash to be signed
         * <para>
         * This method could be used when the data
         * to be signed is in hashed format.
         *
         * @param hashToSign hash to be signed
         * @return this builder
         */
        public SignatureRequestBuilder WithSignableHash(SignableHash hashToSign)
        {
            base.hashToSign = hashToSign;
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
        public SignatureRequestBuilder WithCertificateLevel(string certificateLevel)
        {
            CertificateLevel = certificateLevel;
            return this;
        }

        /**
         * Sets the request's nonce
         * <para>
         * By default the signature's initiation request
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
        public SignatureRequestBuilder WithNonce(string nonce)
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
        public SignatureRequestBuilder WithCapabilities(params Capability[] capabilities)
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
        public SignatureRequestBuilder WithCapabilities(params string[] capabilities)
        {
            this.capabilities = new HashSet<string>(capabilities);
            return this;
        }

        /// <param name="allowedInteractionsOrder">
        /// Preferred order of what dialog to present to user. What actually gets displayed depends on user's device and its software version.
        /// First option from this list that the device is capable of handling is displayed to the user.
        /// </param>
        /// <returns>this builder</returns>
        public SignatureRequestBuilder WithAllowedInteractionsOrder(List<Interaction> allowedInteractionsOrder)
        {
            this.allowedInteractionsOrder = allowedInteractionsOrder;
            return this;
        }

        /// <summary>
        /// Send the signature request and get the response
        /// </summary>
        /// <remarks>This method uses automatic session status polling internally and therefore blocks the current thread until signing is concluded/interupted etc.</remarks>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the signature response</returns>
        /// <exception cref="Exceptions.UserActions.UserAccountNotFoundException">when the user account was not found</exception>
        /// <exception cref="Exceptions.UserActions.UserRefusedException">when the user has refused the session. NB! This exception has subclasses to determine the screen where user pressed cancel.</exception>
        /// <exception cref="Exceptions.UserActions.UserSelectedWrongVerificationCodeException">when user was presented with three control codes and user selected wrong code</exception>
        /// <exception cref="Exceptions.UserActions.SessionTimeoutException">when there was a timeout, i.e. end user did not confirm or refuse the operation within given timeframe</exception>
        /// <exception cref="Exceptions.UserAccounts.DocumentUnusableException">when for some reason, this relying party request cannot be completed. User must either check his/her Smart-ID mobile application or turn to customer support for getting the exact reason.</exception>
        /// <exception cref="Exceptions.Permanent.ServerMaintenanceException">when the server is under maintenance</exception>
        public async Task<SmartIdSignature> SignAsync(CancellationToken cancellationToken = default)
        {
            ValidateParameters();
            string sessionId = await InitiateSigningAsync(cancellationToken);
            SessionStatus sessionStatus = await SessionStatusPoller.FetchFinalSessionStatusAsync(sessionId, cancellationToken);
            return CreateSmartIdSignature(sessionStatus);
        }

        /// <summary>
        /// Send the signature request and get the session Id
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>session Id - later to be used for manual session status polling</returns>
        /// <exception cref="Exceptions.UserActions.UserAccountNotFoundException">when the user account was not found</exception>
        /// <exception cref="Exceptions.Permanent.ServerMaintenanceException">when the server is under maintenance</exception>
        public async Task<string> InitiateSigningAsync(CancellationToken cancellationToken = default)
        {
            ValidateParameters();
            SignatureSessionRequest request = CreateSignatureSessionRequest();
            SignatureSessionResponse response = await GetSignatureResponseAsync(request, cancellationToken);
            return response.SessionId;
        }

        private async Task<SignatureSessionResponse> GetSignatureResponseAsync(SignatureSessionRequest request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(DocumentNumber))
            {
                return await Connector.SignAsync(DocumentNumber, request, cancellationToken);
            }
            else
            {
                return await Connector.SignAsync(SemanticsIdentifier, request, cancellationToken);
            }
        }

        /// <summary>
        /// Get <see cref="SmartIdSignature"/>
        /// from <see cref="SessionStatus"/>
        /// </summary>
        /// <param name="sessionStatus">session status response</param>
        /// <returns>the authentication response</returns>
        /// <exception cref="Exceptions.UserActions.UserRefusedException">when the user has refused the session. NB! This exception has subclasses to determine the screen where user pressed cancel.</exception>
        /// <exception cref="Exceptions.UserActions.SessionTimeoutException">when there was a timeout, i.e. end user did not confirm or refuse the operation within given timeframe</exception>
        /// <exception cref="Exceptions.UserAccounts.DocumentUnusableException">when for some reason, this relying party request cannot be completed.</exception>
        /// <exception cref="UnprocessableSmartIdResponseException">when session status response's result is missing or it has some unknown value</exception>
        public SmartIdSignature CreateSmartIdSignature(SessionStatus sessionStatus)
        {
            ValidateSignatureResponse(sessionStatus);
            SessionSignature sessionSignature = sessionStatus.Signature;

            SmartIdSignature signature = new SmartIdSignature()
            {
                ValueInBase64 = sessionSignature.Value,
                AlgorithmName = sessionSignature.Algorithm,
                DocumentNumber = sessionStatus.Result.DocumentNumber,
                InteractionFlowUsed = sessionStatus.InteractionFlowUsed
            };
            return signature;
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            ValidateAuthSignParameters();
        }

        private void ValidateSignatureResponse(SessionStatus sessionStatus)
        {
            ValidateSessionResult(sessionStatus.Result);
            if (sessionStatus.Signature == null)
            {
                throw new UnprocessableSmartIdResponseException("Signature was not present in the response");
            }
        }

        private SignatureSessionRequest CreateSignatureSessionRequest()
        {
            SignatureSessionRequest request = new SignatureSessionRequest()
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
