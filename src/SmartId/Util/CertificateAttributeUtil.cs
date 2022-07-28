using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId.Util
{
    public static class CertificateAttributeUtil
    {
        /// <summary>
        /// Get Date-of-birth (DoB) from a specific certificate header (if present).
        /// </summary>
        /// <remarks>NB! This attribute may be present on some newer certificates (since ~ May 2021) but not all.</remarks>
        /// <param name="x509Certificate">Certificate to read the date-of-birth attribute from</param>
        /// <returns>Person date of birth or null if this attribute is not set.</returns>
        public static DateTime? GetDateOfBirth(X509Certificate2 x509Certificate)
        {
            DateTime? dateOfBirth = GetDateOfBirthCertificateAttribute(x509Certificate);

            return dateOfBirth?.ToLocalTime().Date;
        }

        private static DateTime? GetDateOfBirthCertificateAttribute(X509Certificate2 x509Certificate)
        {
            //try
            //{
            return GetDateOfBirthFromAttributeInternal(x509Certificate);
            //}
            //catch (IOException | ClassCastException e) {
            //    Trace.TraceInformation("Could not extract date-of-birth from certificate attribute. It seems the attribute does not exist in certificate.");
            //}
            //catch (ParseException e)
            //{
            //    Trace.TraceWarning("Date of birth field existed in certificate but failed to parse the value");
            //}
            //return null;
        }

        private static DateTime? GetDateOfBirthFromAttributeInternal(X509Certificate2 x509Certificate)
        {
            var c =
                new Org.BouncyCastle.X509.X509Certificate(x509Certificate.GetRawCertData());

            var extensionValue = c.GetExtensionValue(X509Extensions.SubjectDirectoryAttributes);

            if (extensionValue is null)
            {
                Debug.WriteLine("subjectDirectoryAttributes field (that carries date-of-birth value) not found from certificate");
                return null;
            }

            foreach (DerSequence param in (Asn1Sequence)Asn1Sequence.FromByteArray(extensionValue.GetOctets()))
            {
                foreach (var val in param)
                {
                    if (val is DerObjectIdentifier id && id.Equals(X509Name.DateOfBirth))
                    {
                        DerSet x = (DerSet)param[1];

                        DerGeneralizedTime time = (DerGeneralizedTime)x[0];
                        return time.ToDateTime();
                    }
                }
            }

            return null;
        }
    }
}
