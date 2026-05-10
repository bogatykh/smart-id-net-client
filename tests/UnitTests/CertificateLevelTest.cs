/*-
 * #%L
 * Smart ID sample Java client
 * #L%
 */

using SK.SmartId.Rest.Dao;
using System;
using Xunit;

namespace SK.SmartId
{
    public class CertificateLevelTest
    {
        [Fact]
        public void BothQualified_IsSameLevelOrHigher()
        {
            Assert.True(CertificateLevelExtensions.TryParse("QUALIFIED", out var level));
            Assert.True(level.IsSameLevelOrHigher(CertificateLevel.QUALIFIED));
        }

        [Fact]
        public void BothAdvanced_IsSameLevelOrHigher()
        {
            Assert.True(CertificateLevelExtensions.TryParse("ADVANCED", out var level));
            Assert.True(level.IsSameLevelOrHigher(CertificateLevel.ADVANCED));
        }

        [Fact]
        public void QualifiedSameOrHigherThanAdvanced()
        {
            Assert.True(CertificateLevel.QUALIFIED.IsSameLevelOrHigher(CertificateLevel.ADVANCED));
        }

        [Fact]
        public void AdvancedNotSameOrHigherThanQualified()
        {
            Assert.False(CertificateLevel.ADVANCED.IsSameLevelOrHigher(CertificateLevel.QUALIFIED));
        }

        [Fact]
        public void QscdMatchesQualifiedLevel()
        {
            Assert.True(CertificateLevelExtensions.TryParse("QSCD", out var qscd));
            Assert.True(qscd.IsSameLevelOrHigher(CertificateLevel.QUALIFIED));
            Assert.True(CertificateLevel.QUALIFIED.IsSameLevelOrHigher(qscd));
        }

        [Fact]
        public void UnknownString_NotSupported()
        {
            Assert.False(CertificateLevelExtensions.IsSupported("SOME UNKNOWN LEVEL"));
        }

        [Fact]
        public void TryParseUnknown_ReturnsFalse()
        {
            Assert.False(CertificateLevelExtensions.TryParse("SOME UNKNOWN LEVEL", out _));
        }

        [Fact]
        public void NullNotSupported()
        {
            Assert.False(CertificateLevelExtensions.IsSupported(null));
        }
    }
}
