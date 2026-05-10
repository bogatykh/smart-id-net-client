/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2024 SK ID Solutions AS
 * #L%
 */

using System.Collections.Generic;
using SK.SmartId.Exceptions.Permanent;
using Xunit;

namespace SK.SmartId
{
    public class RpChallengeGeneratorTest
    {
        [Fact]
        public void Generate_defaultValueUsed()
        {
            var challenge = RpChallengeGenerator.Generate();
            Assert.NotNull(challenge);
            Assert.Equal(64, challenge.GetValue().Length);
        }

        public static IEnumerable<object[]> AllowedLengths()
        {
            yield return new object[] { 32 };
            yield return new object[] { 43 };
            yield return new object[] { 59 };
            yield return new object[] { 64 };
        }

        [Theory]
        [MemberData(nameof(AllowedLengths))]
        public void Generate_providedValuesAreInAllowedRange(int allowedValue)
        {
            var challenge = RpChallengeGenerator.Generate(allowedValue);
            Assert.NotNull(challenge);
            Assert.Equal(allowedValue, challenge.GetValue().Length);
        }

        [Fact]
        public void Generate_providedValueIsLessThanAllowed_throwException()
        {
            var ex = Assert.Throws<SmartIdClientException>(() => RpChallengeGenerator.Generate(31));
            Assert.Equal("Length must be between 32 and 64", ex.Message);
        }

        [Fact]
        public void Generate_providedValueIsMoreThanAllowed_throwException()
        {
            var ex = Assert.Throws<SmartIdClientException>(() => RpChallengeGenerator.Generate(65));
            Assert.Equal("Length must be between 32 and 64", ex.Message);
        }
    }
}
