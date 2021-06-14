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

using SK.SmartId.Rest.Dao;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId.Rest
{
    public class SmartIdConnectorSpy : ISmartIdConnector
    {
        public SessionStatus sessionStatusToRespond;
        public CertificateChoiceResponse certificateChoiceToRespond;
        public SignatureSessionResponse signatureSessionResponseToRespond;
        public AuthenticationSessionResponse authenticationSessionResponseToRespond;

        public string sessionIdUsed;
        public SemanticsIdentifier semanticsIdentifierUsed;
        public string documentNumberUsed;
        public CertificateRequest certificateRequestUsed;
        public SignatureSessionRequest signatureSessionRequestUsed;
        public AuthenticationSessionRequest authenticationSessionRequestUsed;

        public Task<SessionStatus> GetSessionStatusAsync(string sessionId, CancellationToken cancellationToken)
        {
            sessionIdUsed = sessionId;
            return Task.FromResult(sessionStatusToRespond);
        }


        public Task<CertificateChoiceResponse> GetCertificateAsync(string documentNumber, CertificateRequest request, CancellationToken cancellationToken)
        {
            documentNumberUsed = documentNumber;
            certificateRequestUsed = request;
            return Task.FromResult(certificateChoiceToRespond);
        }


        public Task<CertificateChoiceResponse> GetCertificateAsync(SemanticsIdentifier identifier, CertificateRequest request, CancellationToken cancellationToken)
        {
            semanticsIdentifierUsed = identifier;
            certificateRequestUsed = request;
            return Task.FromResult(certificateChoiceToRespond);
        }


        public Task<SignatureSessionResponse> SignAsync(string documentNumber, SignatureSessionRequest request, CancellationToken cancellationToken)
        {
            documentNumberUsed = documentNumber;
            signatureSessionRequestUsed = request;
            return Task.FromResult(signatureSessionResponseToRespond);
        }


        public Task<SignatureSessionResponse> SignAsync(SemanticsIdentifier identifier, SignatureSessionRequest request, CancellationToken cancellationToken)
        {
            semanticsIdentifierUsed = identifier;
            signatureSessionRequestUsed = request;
            return Task.FromResult(signatureSessionResponseToRespond);
        }


        public Task<AuthenticationSessionResponse> AuthenticateAsync(string documentNumber, AuthenticationSessionRequest request, CancellationToken cancellationToken)
        {
            documentNumberUsed = documentNumber;
            authenticationSessionRequestUsed = request;
            return Task.FromResult(authenticationSessionResponseToRespond);
        }


        public Task<AuthenticationSessionResponse> AuthenticateAsync(SemanticsIdentifier identifier, AuthenticationSessionRequest request, CancellationToken cancellationToken)
        {
            semanticsIdentifierUsed = identifier;
            authenticationSessionRequestUsed = request;
            return Task.FromResult(authenticationSessionResponseToRespond);
        }


        public void SetSessionStatusResponseSocketOpenTime(TimeSpan? sessionStatusResponseSocketOpenTime)
        {
        }
    }
}