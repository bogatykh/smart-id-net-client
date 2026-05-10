/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
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
    /// Hash algorithms as named in the Smart-ID API (Java <c>HashAlgorithm</c>).
    /// </summary>
    public enum SmartIdHashAlgorithm
    {
        SHA_256,
        SHA_384,
        SHA_512,
        SHA3_256,
        SHA3_384,
        SHA3_512
    }

    public static class SmartIdHashAlgorithmExtensions
    {
        public static string GetApiAlgorithmName(this SmartIdHashAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case SmartIdHashAlgorithm.SHA_256:
                    return "SHA-256";
                case SmartIdHashAlgorithm.SHA_384:
                    return "SHA-384";
                case SmartIdHashAlgorithm.SHA_512:
                    return "SHA-512";
                case SmartIdHashAlgorithm.SHA3_256:
                    return "SHA3-256";
                case SmartIdHashAlgorithm.SHA3_384:
                    return "SHA3-384";
                case SmartIdHashAlgorithm.SHA3_512:
                    return "SHA3-512";
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
        }

        public static bool TryParse(string name, out SmartIdHashAlgorithm algorithm)
        {
            algorithm = default;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            foreach (SmartIdHashAlgorithm a in Enum.GetValues(typeof(SmartIdHashAlgorithm)))
            {
                if (string.Equals(a.GetApiAlgorithmName(), name, StringComparison.Ordinal))
                {
                    algorithm = a;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Digest size in bytes (salt length for RSASSA-PSS in signing responses; Java <c>HashAlgorithm.getOctetLength</c>).
        /// </summary>
        public static int GetDigestOctetLength(this SmartIdHashAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case SmartIdHashAlgorithm.SHA_256:
                case SmartIdHashAlgorithm.SHA3_256:
                    return 32;
                case SmartIdHashAlgorithm.SHA_384:
                case SmartIdHashAlgorithm.SHA3_384:
                    return 48;
                case SmartIdHashAlgorithm.SHA_512:
                case SmartIdHashAlgorithm.SHA3_512:
                    return 64;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
        }
    }
}
