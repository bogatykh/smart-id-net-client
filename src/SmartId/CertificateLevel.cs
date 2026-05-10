/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 SK ID Solutions AS
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

namespace SK.SmartId
{
    /// <summary>
    /// Signing / authentication certificate levels (Smart-ID v3 API).
    /// </summary>
    public enum CertificateLevel
    {
        ADVANCED = 1,
        QUALIFIED = 2,
        QSCD = 2
    }

    public static class CertificateLevelExtensions
    {
        public static bool IsSupported(string certificateLevel)
        {
            return TryParse(certificateLevel, out _);
        }

        public static bool TryParse(string value, out CertificateLevel level)
        {
            level = default;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            return System.Enum.TryParse(value, ignoreCase: false, out level);
        }

        public static bool IsSameLevelOrHigher(this CertificateLevel certificateLevel, CertificateLevel other)
        {
            return certificateLevel == other || (int)certificateLevel >= (int)other;
        }
    }
}
