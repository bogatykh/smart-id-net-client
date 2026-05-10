/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using System.Text.RegularExpressions;
using SK.SmartId.Common;
using SK.SmartId.Exceptions.Permanent;
using Xunit;

namespace SK.SmartId
{
    public class UrlSafeTokenGeneratorTest
    {
        private static readonly Regex UrlSafePattern = new Regex("^[A-Za-z0-9_-]+$", RegexOptions.Compiled);

        [Fact]
        public void Random_lengthAndCharset()
        {
            string random = UrlSafeTokenGenerator.Random();
            Assert.InRange(random.Length, 22, 86);
            Assert.Matches(UrlSafePattern, random);
        }

        [Fact]
        public void OfLength()
        {
            string random = UrlSafeTokenGenerator.OfLength(22);
            Assert.Equal(22, random.Length);
            Assert.Matches(UrlSafePattern, random);
        }

        [Fact]
        public void RandomBetween()
        {
            string random = UrlSafeTokenGenerator.RandomBetween(22, 24);
            Assert.InRange(random.Length, 22, 24);
            Assert.Matches(UrlSafePattern, random);
        }

        [Theory]
        [InlineData(21, 86)]
        [InlineData(22, 87)]
        [InlineData(86, 22)]
        public void RandomBetween_invalidBounds_throws(int minLength, int maxLength)
        {
            var ex = Assert.Throws<SmartIdClientException>(() => UrlSafeTokenGenerator.RandomBetween(minLength, maxLength));
            Assert.Equal("Length must be between 22 and 86 chars", ex.Message);
        }
    }
}
