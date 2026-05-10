/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId
{
    public class CertificateValidatorImplTest
    {
        [Fact]
        public void Validate_ok()
        {
            X509Certificate2 certificate = TestCertificateUtil.ParseTestCertificate("auth-cert-40504040001.pem.crt");
            IReadOnlyList<X509Certificate2> trust = new List<X509Certificate2>
            {
                TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_SK_ROOT_G1_2021E.pem.crt"),
                TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_of_SK_ID_Solutions_EID-Q_2024E.pem.crt")
            };
            var certificateValidator = new CertificateValidatorImpl(trust);
            var ex = Record.Exception(() => certificateValidator.Validate(certificate));
            Assert.Null(ex);
        }

        [Fact]
        public void Validate_expired()
        {
            X509Certificate2 certificate = TestCertificateUtil.ParseTestCertificate("expired-cert.pem.crt");
            var certificateValidator = new CertificateValidatorImpl(Array.Empty<X509Certificate2>());
            var exception = Assert.Throws<UnprocessableSmartIdResponseException>(() => certificateValidator.Validate(certificate));
            Assert.Equal("Certificate is invalid", exception.Message);
        }

        [Fact]
        public void Validate_notTrusted()
        {
            X509Certificate2 certificate = TestCertificateUtil.ParseTestCertificate("other-auth-cert.pem.crt");
            var certificateValidator = new CertificateValidatorImpl(Array.Empty<X509Certificate2>());
            var exception = Assert.Throws<UnprocessableSmartIdResponseException>(() => certificateValidator.Validate(certificate));
            Assert.Equal("Certificate chain validation failed", exception.Message);
        }
    }
}
