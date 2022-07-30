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

using System;
using Xunit;

namespace SK.SmartId
{
    public class CertificateLevelTest
    {
        [Fact]
        public void testBothCertificateLevelsQualified()
        {
            String certificateLevelString = "QUALIFIED";
            CertificateLevel certificateLevel = new CertificateLevel(certificateLevelString);
            Assert.True(certificateLevel.IsEqualOrAbove(certificateLevelString));
        }

        [Fact]
        public void testBothCertificateLevelsAdvanced()
        {
            String certificateLevelString = "ADVANCED";
            CertificateLevel certificateLevel = new CertificateLevel(certificateLevelString);
            Assert.True(certificateLevel.IsEqualOrAbove(certificateLevelString));
        }

        [Fact]
        public void testFirstCertificateLevelHigher()
        {
            CertificateLevel certificateLevel = new CertificateLevel("QUALIFIED");
            Assert.True(certificateLevel.IsEqualOrAbove("ADVANCED"));
        }

        [Fact]
        public void testFirstCertificateLevelLower()
        {
            CertificateLevel certificateLevel = new CertificateLevel("ADVANCED");
            Assert.False(certificateLevel.IsEqualOrAbove("QUALIFIED"));
        }

        [Fact]
        public void testFirstCertLevelUnknown()
        {
            CertificateLevel certificateLevel = new CertificateLevel("SOME UNKNOWN LEVEL");
            Assert.False(certificateLevel.IsEqualOrAbove("ADVANCED"));
        }

        [Fact]
        public void testSecondCertLevelUnknown()
        {
            CertificateLevel certificateLevel = new CertificateLevel("ADVANCED");
            Assert.False(certificateLevel.IsEqualOrAbove("SOME UNKNOWN LEVEL"));
        }

        [Fact]
        public void certificateLevel_nullArgumentToConstructor()
        {
            Assert.Throws<ArgumentException>("certificateLevel", () => new CertificateLevel(null));
        }
    }
}