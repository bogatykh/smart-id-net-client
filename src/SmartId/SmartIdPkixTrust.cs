/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using BcX509 = Org.BouncyCastle.X509.X509Certificate;

namespace SK.SmartId
{
    /// <summary>
    /// PKIX path building with BouncyCastle (Java <c>CertPathBuilder</c> parity) on netstandard2.0, where
    /// <see cref="System.Security.Cryptography.X509Certificates.X509Chain"/> often fails for SK chains using only <c>ExtraStore</c>.
    /// </summary>
    internal static class SmartIdPkixTrust
    {
        private static readonly X509CertificateParser CertParser = new X509CertificateParser();

        public static bool IsChainTrusted(X509Certificate2 endEntity, IReadOnlyList<X509Certificate2> trustedMaterial)
        {
            if (trustedMaterial == null || trustedMaterial.Count == 0)
            {
                return false;
            }
            try
            {
                BcX509 leaf = ToBcCertificate(endEntity);
                var trustAnchors = new HashSet<TrustAnchor>();
                var pool = new List<BcX509> { leaf };
                foreach (X509Certificate2 x in trustedMaterial)
                {
                    BcX509 bc = ToBcCertificate(x);
                    trustAnchors.Add(new TrustAnchor(bc, null));
                    pool.Add(bc);
                }
                var selector = new X509CertStoreSelector { Certificate = leaf };
                var pkixParams = new PkixBuilderParameters(trustAnchors, selector)
                {
                    IsRevocationEnabled = false,
                    ValidityModel = PkixParameters.ChainValidityModel,
                    Date = DateTime.UtcNow
                };
                pkixParams.AddStoreCert(new SimpleBcCertStore(pool));
                var builder = new PkixCertPathBuilder();
                _ = builder.Build(pkixParams);
                return true;
            }
            catch (PkixCertPathBuilderException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static BcX509 ToBcCertificate(X509Certificate2 cert)
        {
            return CertParser.ReadCertificate(cert.RawData);
        }

        private sealed class SimpleBcCertStore : IStore<BcX509>
        {
            private readonly IList<BcX509> certs;

            public SimpleBcCertStore(IList<BcX509> certs)
            {
                this.certs = certs;
            }

            public IEnumerable<BcX509> EnumerateMatches(ISelector<BcX509> selector)
            {
                foreach (BcX509 c in certs)
                {
                    if (selector == null || selector.Match(c))
                    {
                        yield return c;
                    }
                }
            }
        }
    }
}
