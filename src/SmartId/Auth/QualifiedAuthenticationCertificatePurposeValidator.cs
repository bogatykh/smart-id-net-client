/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Common.Certificate;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId.Auth
{
    public sealed class QualifiedAuthenticationCertificatePurposeValidator : IAuthenticationCertificatePurposeValidator
    {
        private static readonly HashSet<string> QualifiedCertificatePolicyOids = new HashSet<string>(StringComparer.Ordinal)
        {
            "1.3.6.1.4.1.10015.17.2",
            "0.4.0.2042.1.2"
        };

        public void Validate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new SmartIdClientException("Parameter 'certificate' is not provided");
            }

            ValidateCertificateIsQualifiedSmartIdCertificate(certificate);
            SmartIdAuthenticationCertificateValidator.Validate(certificate);
        }

        private void ValidateCertificateIsQualifiedSmartIdCertificate(X509Certificate2 certificate)
        {
            HashSet<string> certificatePolicyOids = CertificateAttributeUtil.GetCertificatePolicyOids(certificate);
            if (certificatePolicyOids.Count == 0)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Certificate does not have certificate policy OIDs and is not a qualified Smart-ID authentication certificate");
            }

            foreach (string required in QualifiedCertificatePolicyOids)
            {
                if (!certificatePolicyOids.Contains(required))
                {
                    Trace.TraceError(
                        "Qualified certificate policy OIDs are missing. Provided certificate policy OIDs: {0}. Required: {1}",
                        string.Join(", ", certificatePolicyOids),
                        string.Join(", ", QualifiedCertificatePolicyOids));
                    throw new UnprocessableSmartIdResponseException("Certificate is not a qualified Smart-ID authentication certificate");
                }
            }
        }
    }
}
