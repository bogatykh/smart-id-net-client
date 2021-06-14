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
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserAccounts;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using System.Collections.Generic;

namespace SK.SmartId
{
    public abstract class SmartIdRequestBuilder
    {
        private readonly ISmartIdConnector _connector;
        private readonly SessionStatusPoller _sessionStatusPoller;

        protected SignableData dataToSign;
        protected SignableHash hashToSign;
        protected HashSet<string> capabilities;
        protected List<Interaction> allowedInteractionsOrder;

        protected SmartIdRequestBuilder(ISmartIdConnector connector, SessionStatusPoller sessionStatusPoller)
        {
            _connector = connector;
            _sessionStatusPoller = sessionStatusPoller;
        }

        protected virtual void ValidateParameters()
        {
            if (string.IsNullOrEmpty(RelyingPartyUUID))
            {
                throw new SmartIdClientException("Parameter relyingPartyUUID must be set");
            }
            if (string.IsNullOrEmpty(RelyingPartyName))
            {
                throw new SmartIdClientException("Parameter relyingPartyName must be set");
            }
            if (Nonce != null && Nonce.Length > 30)
            {
                throw new SmartIdClientException("Nonce cannot be longer that 30 chars. You supplied: '" + Nonce + "'");
            }

            int identifierCount = GetIdentifiersCount();

            if (identifierCount == 0)
            {
                throw new SmartIdClientException("Either documentNumber or semanticsIdentifier must be set");
            }
            else if (identifierCount > 1)
            {
                throw new SmartIdClientException("Exactly one of documentNumber or semanticsIdentifier must be set");
            }
        }

        protected void ValidateAuthSignParameters()
        {
            if (!IsHashSet() && !IsSignableDataSet())
            {
                throw new SmartIdClientException("Either dataToSign or hash with hashType must be set");
            }
            ValidateAllowedInteractionOrder();
        }

        private void ValidateAllowedInteractionOrder()
        {
            if (AllowedInteractionsOrder == null || AllowedInteractionsOrder.Count == 0)
            {
                throw new SmartIdClientException("Missing or empty mandatory parameter allowedInteractionsOrder");
            }

            foreach (var interaction in AllowedInteractionsOrder)
            {
                interaction.Validate();
            }
        }

        private int GetIdentifiersCount()
        {
            int identifierCount = 0;
            if (!string.IsNullOrEmpty(DocumentNumber))
            {
                identifierCount++;
            }
            if (HasSemanticsIdentifier())
            {
                identifierCount++;
            }
            return identifierCount;
        }

        protected void ValidateSessionResult(SessionResult result)
        {
            if (result == null)
            {
                throw new UnprocessableSmartIdResponseException("Result is missing in the session status response");
            }
            string endResult = result.EndResult.ToUpperInvariant();

            switch (endResult)
            {
                case "OK":
                    return;
                case "USER_REFUSED":
                    throw new UserRefusedException();
                case "TIMEOUT":
                    throw new SessionTimeoutException();
                case "DOCUMENT_UNUSABLE":
                    throw new DocumentUnusableException();
                case "WRONG_VC":
                    throw new UserSelectedWrongVerificationCodeException();
                case "REQUIRED_INTERACTION_NOT_SUPPORTED_BY_APP":
                    throw new RequiredInteractionNotSupportedByAppException();
                case "USER_REFUSED_CERT_CHOICE":
                    throw new UserRefusedCertChoiceException();
                case "USER_REFUSED_DISPLAYTEXTANDPIN":
                    throw new UserRefusedDisplayTextAndPinException();
                case "USER_REFUSED_VC_CHOICE":
                    throw new UserRefusedVerificationChoiceException();
                case "USER_REFUSED_CONFIRMATIONMESSAGE":
                    throw new UserRefusedConfirmationMessageException();
                case "USER_REFUSED_CONFIRMATIONMESSAGE_WITH_VC_CHOICE":
                    throw new UserRefusedConfirmationMessageWithVerificationChoiceException();
                default:
                    throw new UnprocessableSmartIdResponseException("Session status end result is '" + endResult + "'");
            }
        }

        protected bool HasSemanticsIdentifier()
        {
            return SemanticsIdentifier != null;
        }

        protected bool IsHashSet()
        {
            return hashToSign != null && hashToSign.AreFieldsFilled();
        }

        protected bool IsSignableDataSet()
        {
            return dataToSign != null;
        }

        protected string GetHashTypeString()
        {
            return HashType.HashTypeName;
        }

        protected HashType HashType
        {
            get
            {
                if (hashToSign != null)
                {
                    return hashToSign.HashType;
                }
                return dataToSign.HashType;
            }
        }

        protected string GetHashInBase64()
        {
            if (hashToSign != null)
            {
                return hashToSign.HashInBase64;
            }
            return dataToSign.CalculateHashInBase64();
        }

        public ISmartIdConnector Connector => _connector;

        protected SessionStatusPoller SessionStatusPoller => _sessionStatusPoller;

        protected string RelyingPartyUUID { get; set; }

        protected string RelyingPartyName { get; set; }

        protected string DocumentNumber { get; set; }

        protected string CertificateLevel { get; set; }

        protected string Nonce { get; set; }

        public SemanticsIdentifier SemanticsIdentifier { get; protected set; }

        public HashSet<string> Capabilities => capabilities;

        public List<Interaction> AllowedInteractionsOrder => allowedInteractionsOrder;
    }
}