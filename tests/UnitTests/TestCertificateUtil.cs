/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>
    /// PEM → API-style single-line base64 (Java test <c>CertificateUtil.getEncodedCertificateData</c>).
    /// </summary>
    internal static class TestCertificateUtil
    {
        public static string GetEncodedCertificateData(string fileNameUnderTestCerts)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Resources", "test-certs", fileNameUnderTestCerts);
            string pem = File.ReadAllText(path).Trim();
            return pem.Replace("-----BEGIN CERTIFICATE-----", "", System.StringComparison.Ordinal)
                .Replace("-----END CERTIFICATE-----", "", System.StringComparison.Ordinal)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);
        }

        public static X509Certificate2 ParseTestCertificate(string fileNameUnderTestCerts) =>
            CertificateParser.ParseX509Certificate(GetEncodedCertificateData(fileNameUnderTestCerts));

        /// <summary>PEM file copied next to the test assembly under <c>Resources/</c>.</summary>
        public static X509Certificate2 ParsePemCertificateFromOutput(string relativePathFromBaseDirectory)
        {
            string path = Path.Combine(AppContext.BaseDirectory, relativePathFromBaseDirectory);
            string pem = File.ReadAllText(path).Trim();
            string b64 = pem.Replace("-----BEGIN CERTIFICATE-----", "", StringComparison.Ordinal)
                .Replace("-----END CERTIFICATE-----", "", StringComparison.Ordinal)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);
            return CertificateParser.ParseX509Certificate(b64);
        }
    }
}
