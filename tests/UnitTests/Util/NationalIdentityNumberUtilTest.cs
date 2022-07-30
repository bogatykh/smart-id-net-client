/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2022 SK ID Solutions AS
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
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId.Util
{
    public class NationalIdentityNumberUtilTest
    {
        [Fact]
        public void getDateOfBirthFromIdCode_estonianIdCode_returns()
        {
            X509Certificate2 eeCertificate = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_EE));

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(eeCertificate);


            var dateOfBirth = NationalIdentityNumberUtil.GetDateOfBirth(identity);

            Assert.NotNull(dateOfBirth);
            Assert.Equal(new DateTime(1801, 1, 1), dateOfBirth);
        }

        [Fact]
        public void getDateOfBirthFromIdCode_latvianIdCode_returns()
        {
            X509Certificate2 lvCertificate = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_LV_DOB_03_APRIL_1903));

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(lvCertificate);

            var dateOfBirth = NationalIdentityNumberUtil.GetDateOfBirth(identity);

            Assert.NotNull(dateOfBirth);
            Assert.Equal(new DateTime(1903, 4, 3), dateOfBirth);
        }

        [Fact]
        public void getDateOfBirthFromIdCode_lithuanianIdCode_returns()
        {
            X509Certificate2 ltCertificate = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_LT));

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(ltCertificate);

            var dateOfBirth = NationalIdentityNumberUtil.GetDateOfBirth(identity);

            Assert.NotNull(dateOfBirth);
            Assert.Equal(new DateTime(1960, 9, 6), dateOfBirth);
        }
        [Fact]
        public void parseLvDateOfBirth_withoutDateOfBirth_returnsNull()
        {
            var birthDate = NationalIdentityNumberUtil.ParseLvDateOfBirth("321205-1234");
            Assert.Null(birthDate);
        }

        [Fact]
        public void parseLvDateOfBirth_21century()
        {
            var birthDate = NationalIdentityNumberUtil.ParseLvDateOfBirth("131205-2234");
            Assert.Equal(new DateTime(2005, 12, 13), birthDate);
        }

        [Fact]
        public void parseLvDateOfBirth_20century()
        {
            var birthDate = NationalIdentityNumberUtil.ParseLvDateOfBirth("131265-1234");
            Assert.Equal(new DateTime(1965, 12, 13), birthDate);
        }

        [Fact]
        public void parseLvDateOfBirth_19century()
        {
            var birthDate = NationalIdentityNumberUtil.ParseLvDateOfBirth("131265-0234");
            Assert.Equal(new DateTime(1865, 12, 13), birthDate);
        }

        [Fact]
        public void parseLvDateOfBirth_invalidMonth_throwsException()
        {
            var exception = Assert.Throws<UnprocessableSmartIdResponseException>(() => NationalIdentityNumberUtil.ParseLvDateOfBirth("131365-1234"));

            Assert.Equal("Unable get birthdate from Latvian personal code 131365-1234", exception.Message);
        }

        [Fact]
        public void parseLvDateOfBirth_invalidIdCode_throwsException()
        {
            var exception = Assert.Throws<UnprocessableSmartIdResponseException>(() => NationalIdentityNumberUtil.ParseLvDateOfBirth("331265-0234"));
            Assert.Equal("Unable get birthdate from Latvian personal code 331265-0234", exception.Message);
        }

        [Fact]
        public void getDateOfBirthFromIdCode_sweden_returnsNull()
        {
            AuthenticationIdentity identity = new AuthenticationIdentity();
            identity.Country = "SE";
            identity.IdentityNumber = "1995012-79039";

            Assert.Null(NationalIdentityNumberUtil.GetDateOfBirth(identity));
        }

        [Fact]
        public void getDateOfBirthFromIdCode_poland_returnsNull()
        {
            AuthenticationIdentity identity = new AuthenticationIdentity();
            identity.Country = "PL";
            identity.IdentityNumber = "64120301283";

            Assert.Null(NationalIdentityNumberUtil.GetDateOfBirth(identity));
        }
    }
}
