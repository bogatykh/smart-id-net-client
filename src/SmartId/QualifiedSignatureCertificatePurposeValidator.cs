/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Org.BouncyCastle.X509;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Util;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>
    /// Qualified signature certificate purpose (Java <c>QualifiedSignatureCertificatePurposeValidator</c>).
    /// </summary>
    public sealed class QualifiedSignatureCertificatePurposeValidator : ISignatureCertificatePurposeValidator
    {
        private static readonly HashSet<string> QualifiedCertificatePolicyOids = new HashSet<string>(StringComparer.Ordinal)
        {
            "1.3.6.1.4.1.10015.17.2",
            "0.4.0.194112.1.2"
        };

        private static readonly DerObjectIdentifier IdEtsiQcsQcType = new DerObjectIdentifier("0.4.0.1862.1.6");
        private static readonly DerObjectIdentifier IdEtsiQctEsign = new DerObjectIdentifier("0.4.0.1862.1.6.1");

        public void Validate(X509Certificate2 certificate)
        {
            ValidateCertificateHasQualifiedSmartIdCertificatePolicies(certificate);
            ValidateCertificateCanBeUsedForSigning(certificate);
            ValidateCertificateCanBeUsedForQualifiedElectronicSignature(certificate);
        }

        private static void ValidateCertificateHasQualifiedSmartIdCertificatePolicies(X509Certificate2 certificate)
        {
            HashSet<string> policyOids = CertificateAttributeUtil.GetCertificatePolicyOids(certificate);
            if (policyOids.Count == 0)
            {
                throw new UnprocessableSmartIdResponseException("Certificate does not have certificate policy OIDs");
            }
            foreach (var required in QualifiedCertificatePolicyOids)
            {
                if (!policyOids.Contains(required))
                {
                    throw new UnprocessableSmartIdResponseException(
                        "Certificate does not contain required qualified certificate policy OIDs");
                }
            }
        }

        private static void ValidateCertificateCanBeUsedForSigning(X509Certificate2 certificate)
        {
            if (!CertificateAttributeUtil.HasNonRepudiationKeyUsage(certificate))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Certificate does not have Non-Repudiation set in 'KeyUsage' extension");
            }
        }

        private static void ValidateCertificateCanBeUsedForQualifiedElectronicSignature(X509Certificate2 certificate)
        {
            var bcCert = new X509CertificateParser().ReadCertificate(certificate.RawData);
            Asn1OctetString extVal = bcCert.GetExtensionValue(X509Extensions.QCStatements);
            if (extVal == null)
            {
                throw new UnprocessableSmartIdResponseException("Certificate does not have 'QCStatements' extension");
            }
            if (!HasElectronicSigningOid(extVal))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Certificate does not have electronic signature OID (" + IdEtsiQctEsign.Id + ") in QCStatements extension.");
            }
        }

        private static bool HasElectronicSigningOid(Asn1OctetString extensionValue)
        {
            Asn1Sequence qcStatements;
            try
            {
                qcStatements = Asn1Sequence.GetInstance(Asn1Object.FromByteArray(extensionValue.GetOctets()));
            }
            catch (Exception ex)
            {
                throw new SmartIdClientException("Unable to parse QCStatements extension", ex);
            }

            for (int i = 0; i < qcStatements.Count; i++)
            {
                QCStatement qs = QCStatement.GetInstance(qcStatements[i]);
                if (!IdEtsiQcsQcType.Equals(qs.StatementId))
                {
                    continue;
                }
                Asn1Sequence typeSeq = Asn1Sequence.GetInstance(qs.StatementInfo);
                if (typeSeq == null)
                {
                    return false;
                }
                for (int j = 0; j < typeSeq.Count; j++)
                {
                    DerObjectIdentifier typeOid = DerObjectIdentifier.GetInstance(typeSeq[j]);
                    if (IdEtsiQctEsign.Equals(typeOid))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
