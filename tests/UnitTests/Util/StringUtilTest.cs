/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Util;
using Xunit;

namespace SK.SmartId.Util
{
    public class StringUtilTest
    {
        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("x", false)]
        public void IsEmpty(string value, bool expected)
        {
            Assert.Equal(expected, StringUtil.IsEmpty(value));
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("x", true)]
        public void IsNotEmpty(string value, bool expected)
        {
            Assert.Equal(expected, StringUtil.IsNotEmpty(value));
        }

        [Fact]
        public void OrEmpty_null_returnsEmptyString()
        {
            Assert.Equal("", StringUtil.OrEmpty(null));
        }

        [Fact]
        public void OrEmpty_nonNull_returnsSameReference()
        {
            const string s = "hello";
            Assert.Same(s, StringUtil.OrEmpty(s));
        }
    }
}
