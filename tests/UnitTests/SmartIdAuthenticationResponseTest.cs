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
using System;
using System.Text;
using Xunit;

namespace SK.SmartId
{
    public class SmartIdAuthenticationResponseTest
    {
        [Fact]
        public void GetSignatureValueInBase64()
        {
            SmartIdAuthenticationResponse AuthenticationResponse = new SmartIdAuthenticationResponse();
            AuthenticationResponse.SignatureValueInBase64 = "SGVsbG8gU21hcnQtSUQgc2lnbmF0dXJlIQ==";
            Assert.Equal("SGVsbG8gU21hcnQtSUQgc2lnbmF0dXJlIQ==", AuthenticationResponse.SignatureValueInBase64);
        }

        [Fact]
        public void GetSignatureValueInBytes()
        {
            SmartIdAuthenticationResponse AuthenticationResponse = new SmartIdAuthenticationResponse();
            AuthenticationResponse.SignatureValueInBase64 = "VGVyZSBhbGxraXJpIQ==";
            Assert.Equal(Encoding.UTF8.GetBytes("Tere allkiri!"), AuthenticationResponse.SignatureValue);
        }

        [Fact]
        public void IncorrectBase64StringShouldThrowException()
        {
            SmartIdAuthenticationResponse AuthenticationResponse = new SmartIdAuthenticationResponse();
            AuthenticationResponse.SignatureValueInBase64 = "!IsNotValidBase64Character";

            Assert.Throws<UnprocessableSmartIdResponseException>(() => AuthenticationResponse.SignatureValue);
        }

        [Fact]
        public void GetCertificate()
        {
            SmartIdAuthenticationResponse AuthenticationResponse = new SmartIdAuthenticationResponse();
            AuthenticationResponse.Certificate = CertificateParser.ParseX509Certificate(DummyData.CERTIFICATE);
            Assert.Equal(DummyData.CERTIFICATE, Convert.ToBase64String(AuthenticationResponse.Certificate.GetRawCertData()));
        }
    }
}