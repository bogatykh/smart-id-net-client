﻿/*-
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

using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId.Util
{
    public class CertificateAttributeUtilTest
    {
        [Fact]
        public void getDateOfBirthFromCertificateAttribute_datePresent_returns()
        {
            X509Certificate2 certificateWithDob = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_LV_WITH_DOB));

            var dateOfBirthCertificateAttribute = CertificateAttributeUtil.GetDateOfBirth(certificateWithDob);

            Assert.NotNull(dateOfBirthCertificateAttribute);
            Assert.Equal(new DateTime(1903, 3, 3), dateOfBirthCertificateAttribute);
        }

        [Fact]
        public void getDateOfBirthFromCertificateAttribute_dateNotPresent_returnsEmpty()
        {
            X509Certificate2 certificateWithoutDobAttribute = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_LV));

            var dateOfBirthCertificateAttribute = CertificateAttributeUtil.GetDateOfBirth(certificateWithoutDobAttribute);

            Assert.Null(dateOfBirthCertificateAttribute);
        }

    }
}
