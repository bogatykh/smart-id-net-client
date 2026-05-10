/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * %%
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * #L%
 */

using System;

namespace SK.SmartId
{
    /// <summary>
    /// Signature algorithms for signing sessions (Java <c>SigningSignatureAlgorithm</c>).
    /// </summary>
    public enum SigningSignatureAlgorithm
    {
        RSASSA_PSS,
        SHA256_WITH_RSA_ENCRYPTION,
        SHA384_WITH_RSA_ENCRYPTION,
        SHA512_WITH_RSA_ENCRYPTION
    }

    public static class SigningSignatureAlgorithmExtensions
    {
        public static string GetAlgorithmName(this SigningSignatureAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case SigningSignatureAlgorithm.RSASSA_PSS:
                    return "rsassa-pss";
                case SigningSignatureAlgorithm.SHA256_WITH_RSA_ENCRYPTION:
                    return "sha256WithRSAEncryption";
                case SigningSignatureAlgorithm.SHA384_WITH_RSA_ENCRYPTION:
                    return "sha384WithRSAEncryption";
                case SigningSignatureAlgorithm.SHA512_WITH_RSA_ENCRYPTION:
                    return "sha512WithRSAEncryption";
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
        }

        public static bool IsLegacyRsa(this SigningSignatureAlgorithm algorithm) =>
            algorithm != SigningSignatureAlgorithm.RSASSA_PSS;

        public static SmartIdHashAlgorithm? GetHashAlgorithmForLegacy(this SigningSignatureAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case SigningSignatureAlgorithm.SHA256_WITH_RSA_ENCRYPTION:
                    return SmartIdHashAlgorithm.SHA_256;
                case SigningSignatureAlgorithm.SHA384_WITH_RSA_ENCRYPTION:
                    return SmartIdHashAlgorithm.SHA_384;
                case SigningSignatureAlgorithm.SHA512_WITH_RSA_ENCRYPTION:
                    return SmartIdHashAlgorithm.SHA_512;
                default:
                    return null;
            }
        }

        public static bool TryParseFromApiAlgorithmName(string algorithmName, out SigningSignatureAlgorithm algorithm)
        {
            algorithm = default;
            if (string.IsNullOrEmpty(algorithmName))
            {
                return false;
            }
            foreach (SigningSignatureAlgorithm a in Enum.GetValues(typeof(SigningSignatureAlgorithm)))
            {
                if (string.Equals(a.GetAlgorithmName(), algorithmName, StringComparison.OrdinalIgnoreCase))
                {
                    algorithm = a;
                    return true;
                }
            }
            return false;
        }

        public static bool IsSupportedApiAlgorithmName(string algorithmName) =>
            TryParseFromApiAlgorithmName(algorithmName, out _);

        public static bool IsLegacyRsaApiAlgorithmName(string algorithmName) =>
            TryParseFromApiAlgorithmName(algorithmName, out var a) && a.IsLegacyRsa();
    }
}
