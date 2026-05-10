/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Util;
using System;
using System.Collections.Generic;

namespace SK.SmartId.Common.Certificate
{
    /// <summary>
    /// Non-qualified Smart-ID certificate policy checks (Java <c>NonQualifiedSmartIdCertificateValidator</c>).
    /// </summary>
    public static class NonQualifiedSmartIdCertificateValidator
    {
        private static readonly HashSet<string> NonQualifiedCertificatePolicyOids = new HashSet<string>(StringComparer.Ordinal)
        {
            "1.3.6.1.4.1.10015.17.1",
            "0.4.0.2042.1.1"
        };

        public static void Validate(System.Security.Cryptography.X509Certificates.X509Certificate2 certificate)
        {
            HashSet<string> policyOids = CertificateAttributeUtil.GetCertificatePolicyOids(certificate);
            if (policyOids.Count == 0)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Certificate does not have certificate policy OIDs and is not a non-qualified Smart-ID certificate");
            }
            foreach (var required in NonQualifiedCertificatePolicyOids)
            {
                if (!policyOids.Contains(required))
                {
                    throw new UnprocessableSmartIdResponseException("Certificate is not a non-qualified Smart-ID certificate");
                }
            }
        }
    }
}
