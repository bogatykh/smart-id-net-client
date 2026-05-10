/*-
 * #%L
 * Smart ID .NET client tests
 * %%
 * Port of Java test helper <c>ee.sk.smartid.InvalidCertificateGenerator</c>.
 * #L%
 */

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId.TestSupport
{
    internal static class InvalidCertificateGenerator
    {
        public static CertificatePolicies CreateCertificatePolicies(params PolicyInformation[] policyInformations)
        {
            Asn1EncodableVector vec = new Asn1EncodableVector();
            foreach (var pi in policyInformations)
            {
                vec.Add(pi);
            }
            return CertificatePolicies.GetInstance(new DerSequence(vec));
        }

        public static CertificateBuilder NewBuilder() => new CertificateBuilder();

        /// <summary>Java-style alias for <see cref="NewBuilder"/>.</summary>
        public static CertificateBuilder Builder() => NewBuilder();

        internal sealed class CertificateBuilder
        {
            private CertificatePolicies policies;
            private ExtendedKeyUsage extendedKeyUsage;
            private KeyUsage keyUsage;
            private QCStatement qcStatement;

            public CertificateBuilder WithPolicies(CertificatePolicies policies)
            {
                this.policies = policies;
                return this;
            }

            public CertificateBuilder WithExtendedKeyUsage(ExtendedKeyUsage extendedKeyUsage)
            {
                this.extendedKeyUsage = extendedKeyUsage;
                return this;
            }

            public CertificateBuilder WithKeyUsage(KeyUsage keyUsage)
            {
                this.keyUsage = keyUsage;
                return this;
            }

            public CertificateBuilder WithQcStatement(QCStatement qcStatement)
            {
                this.qcStatement = qcStatement;
                return this;
            }

            public X509Certificate2 CreateCertificate()
            {
                var random = new SecureRandom();
                var keyGen = new RsaKeyPairGenerator();
                keyGen.Init(new KeyGenerationParameters(random, 2048));
                AsymmetricCipherKeyPair kp = keyGen.GenerateKeyPair();

                var certGen = new X509V3CertificateGenerator();
                certGen.SetSerialNumber(BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random));
                certGen.SetIssuerDN(new X509Name("CN=MyRootCA,O=MyOrg,C=US"));
                certGen.SetNotBefore(DateTime.UtcNow.AddMinutes(-1));
                certGen.SetNotAfter(DateTime.UtcNow.AddYears(1));
                certGen.SetSubjectDN(new X509Name("CN=TestCert,O=MyOrg,C=US"));
                certGen.SetPublicKey(kp.Public);

                if (policies != null)
                {
                    certGen.AddExtension(X509Extensions.CertificatePolicies, false, policies);
                }
                if (extendedKeyUsage != null)
                {
                    certGen.AddExtension(X509Extensions.ExtendedKeyUsage, false, extendedKeyUsage);
                }
                if (keyUsage != null)
                {
                    certGen.AddExtension(X509Extensions.KeyUsage, true, keyUsage);
                }
                if (qcStatement != null)
                {
                    certGen.AddExtension(X509Extensions.QCStatements, false, new DerSequence(qcStatement));
                }

                Org.BouncyCastle.Crypto.ISignatureFactory factory =
                    new Asn1SignatureFactory("SHA256WithRSA", kp.Private, random);
                Org.BouncyCastle.X509.X509Certificate bcCert = certGen.Generate(factory);
                return X509CertificateLoader.LoadCertificate(bcCert.GetEncoded());
            }
        }
    }
}
