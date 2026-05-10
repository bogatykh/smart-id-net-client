/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>
    /// Loads the default SK Smart-ID PEM trust anchors embedded in this assembly (shared by validators).
    /// </summary>
    internal static class EmbeddedSmartIdTrustedCaCertificates
    {
        private static readonly string[] ResourceNames =
        {
            "SK.SmartId.Resources.EID-SK_2016.pem.crt",
            "SK.SmartId.Resources.NQ-SK_2016.pem.crt",
            "SK.SmartId.Resources.TEST_of_EID-SK_2016.pem.crt",
            "SK.SmartId.Resources.TEST_of_NQ-SK_2016.pem.crt"
        };

        public static IReadOnlyList<X509Certificate2> LoadDefaultPemCertificates()
        {
            var assembly = typeof(EmbeddedSmartIdTrustedCaCertificates).GetTypeInfo().Assembly;
            var list = new List<X509Certificate2>();
            foreach (var resourceName in ResourceNames)
            {
                using (Stream resource = assembly.GetManifestResourceStream(resourceName))
                {
                    byte[] buffer = new byte[resource.Length];
                    int offset = 0;
                    int r;
                    while ((r = resource.Read(buffer, offset, buffer.Length - offset)) > 0)
                    {
                        offset += r;
                    }
                    list.Add(new X509Certificate2(buffer));
                }
            }
            return list;
        }
    }
}
