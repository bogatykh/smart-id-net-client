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
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserAccounts;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId
{
    public class CertificateByDocumentNumberRequestBuilder
    {
        private static readonly Regex Base64Pattern = new Regex("^[A-Za-z0-9+/]+={0,2}$", RegexOptions.Compiled);

        private readonly ISmartIdConnector connector;
        private string documentNumber;
        private string relyingPartyUUID;
        private string relyingPartyName;
        private CertificateLevel certificateLevel = CertificateLevel.QUALIFIED;

        public CertificateByDocumentNumberRequestBuilder(ISmartIdConnector connector)
        {
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public CertificateByDocumentNumberRequestBuilder WithDocumentNumber(string documentNumber)
        {
            this.documentNumber = documentNumber;
            return this;
        }

        public CertificateByDocumentNumberRequestBuilder WithRelyingPartyUUID(string relyingPartyUUID)
        {
            this.relyingPartyUUID = relyingPartyUUID;
            return this;
        }

        public CertificateByDocumentNumberRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            this.relyingPartyName = relyingPartyName;
            return this;
        }

        public CertificateByDocumentNumberRequestBuilder WithCertificateLevel(CertificateLevel certificateLevel)
        {
            this.certificateLevel = certificateLevel;
            return this;
        }

        public async Task<CertificateByDocumentNumberResult> GetCertificateByDocumentNumberAsync(CancellationToken cancellationToken = default)
        {
            ValidateRequestParameters();
            var request = new CertificateByDocumentNumberRequest
            {
                RelyingPartyUUID = relyingPartyUUID,
                RelyingPartyName = relyingPartyName,
                CertificateLevel = certificateLevel.ToString()
            };
            var response = await connector.GetCertificateByDocumentNumberAsync(documentNumber, request, cancellationToken);
            ValidateResponseParameters(response);

            CertificateLevelExtensions.TryParse(response.Cert.CertificateLevel, out var parsedLevel);
            return new CertificateByDocumentNumberResult(
                parsedLevel,
                CertificateParser.ParseX509Certificate(response.Cert.Value));
        }

        private void ValidateRequestParameters()
        {
            if (string.IsNullOrEmpty(documentNumber))
            {
                throw new SmartIdClientException("Value for 'documentNumber' cannot be empty");
            }
            if (string.IsNullOrEmpty(relyingPartyUUID))
            {
                throw new SmartIdClientException("Value for 'relyingPartyUUID' cannot be empty");
            }
            if (string.IsNullOrEmpty(relyingPartyName))
            {
                throw new SmartIdClientException("Value for 'relyingPartyName' cannot be empty");
            }
        }

        private void ValidateResponseParameters(CertificateResponse certificateResponse)
        {
            if (certificateResponse == null)
            {
                throw new UnprocessableSmartIdResponseException("Queried certificate response is not provided");
            }
            ValidateState(certificateResponse);

            if (certificateResponse.Cert == null)
            {
                throw new UnprocessableSmartIdResponseException("Queried certificate response field 'cert' is missing");
            }
            ValidateCertificateLevel(certificateResponse);

            if (string.IsNullOrEmpty(certificateResponse.Cert.Value))
            {
                throw new UnprocessableSmartIdResponseException("Queried certificate response field 'cert.value' is missing");
            }
            if (!Base64Pattern.IsMatch(certificateResponse.Cert.Value))
            {
                throw new UnprocessableSmartIdResponseException("Queried certificate response field 'cert.value' does not have Base64-encoded value");
            }
        }

        private static void ValidateState(CertificateResponse certificateResponse)
        {
            var state = certificateResponse.State;
            if (string.IsNullOrEmpty(state))
            {
                throw new UnprocessableSmartIdResponseException("Queried certificate response field 'state' is missing");
            }
            if (!CertificateStateExtensions.IsSupported(state))
            {
                throw new UnprocessableSmartIdResponseException("Queried certificate response field 'state' has unsupported value");
            }
            if (Enum.TryParse<CertificateState>(state, ignoreCase: false, out var st) && st == CertificateState.DOCUMENT_UNUSABLE)
            {
                throw new DocumentUnusableException();
            }
        }

        private void ValidateCertificateLevel(CertificateResponse certificateResponse)
        {
            var certificateLevelStr = certificateResponse.Cert.CertificateLevel;
            if (string.IsNullOrEmpty(certificateLevelStr))
            {
                throw new UnprocessableSmartIdResponseException("Queried certificate response field 'cert.certificateLevel' is missing");
            }
            if (!CertificateLevelExtensions.TryParse(certificateLevelStr, out var responseLevel))
            {
                throw new UnprocessableSmartIdResponseException("Queried certificate response field 'cert.certificateLevel' has unsupported value");
            }
            var requestedLevel = certificateLevel;
            if (!responseLevel.IsSameLevelOrHigher(requestedLevel))
            {
                throw new UnprocessableSmartIdResponseException("Queried certificate has lower level than requested");
            }
        }
    }
}
