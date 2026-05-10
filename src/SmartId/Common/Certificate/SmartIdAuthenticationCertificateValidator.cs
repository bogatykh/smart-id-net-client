/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using SK.SmartId.Exceptions;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId.Common.Certificate
{
    /// <summary>
    /// Smart-ID authentication certificate checks (Java <c>SmartIdAuthenticationCertificateValidator</c>).
    /// </summary>
    public static class SmartIdAuthenticationCertificateValidator
    {
        private const int IndexDigitalSignature = 0;
        private const int IndexKeyEncipherment = 2;
        private const int IndexDataEncipherment = 3;

        private const string EkuAfterApril2025 = "1.3.6.1.4.1.62306.5.7.0";
        private const string EkuBeforeApril2025 = "1.3.6.1.5.5.7.3.2";

        public static void Validate(X509Certificate2 certificate)
        {
            if (!(IsAfterApril2025Certificates(certificate) || IsBeforeApril2025Certificates(certificate)))
            {
                throw new UnprocessableSmartIdResponseException("Provided certificate cannot be used for authentication");
            }
        }

        private static bool IsAfterApril2025Certificates(X509Certificate2 certificate)
        {
            if (!HasExtendedKey(certificate, EkuAfterApril2025))
            {
                return false;
            }

            bool[] keyUsage = GetKeyUsage(certificate);
            if (!(keyUsage != null && keyUsage.Length > IndexDigitalSignature && keyUsage[IndexDigitalSignature]))
            {
                Trace.WriteLine("Certificate has invalid values for key usage (post-April-2025 profile).");
                return false;
            }

            return true;
        }

        private static bool IsBeforeApril2025Certificates(X509Certificate2 certificate)
        {
            if (!HasExtendedKey(certificate, EkuBeforeApril2025))
            {
                return false;
            }

            bool[] keyUsage = GetKeyUsage(certificate);
            if (!(keyUsage != null
                    && keyUsage.Length > IndexDataEncipherment
                    && keyUsage[IndexDigitalSignature]
                    && keyUsage[IndexKeyEncipherment]
                    && keyUsage[IndexDataEncipherment]))
            {
                Trace.WriteLine("Certificate has invalid values for key usage (pre-April-2025 profile).");
                return false;
            }

            return true;
        }

        private static bool[] GetKeyUsage(X509Certificate2 certificate)
        {
            var bcCert = new X509CertificateParser().ReadCertificate(certificate.RawData);
            return bcCert.GetKeyUsage();
        }

        private static bool HasExtendedKey(X509Certificate2 certificate, string oid)
        {
            try
            {
                var bcCert = new X509CertificateParser().ReadCertificate(certificate.RawData);
                Asn1OctetString extVal = bcCert.GetExtensionValue(X509Extensions.ExtendedKeyUsage);
                if (extVal == null)
                {
                    Trace.WriteLine("Certificate does not have extended key usage for authentication.");
                    return false;
                }

                var eku = ExtendedKeyUsage.GetInstance(Asn1Object.FromByteArray(extVal.GetOctets()));
                foreach (object o in eku.GetAllUsages())
                {
                    if (o is DerObjectIdentifier d && string.Equals(d.Id, oid, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                Trace.WriteLine("Certificate does not have extended key usage for authentication.");
                return false;
            }
            catch (Exception ex)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Provided certificate for is incorrect and cannot be used for authentication",
                    ex);
            }
        }
    }
}
