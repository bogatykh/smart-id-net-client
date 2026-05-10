/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using System;
using SK.SmartId.Util;
using Xunit;

namespace SK.SmartId.Util
{
    public class SetUtilTest
    {
        [Fact]
        public void ToSet_nullArray_throws()
        {
            Assert.Throws<ArgumentNullException>(() => SetUtil.ToSet(null));
        }

        [Fact]
        public void ToSet_emptyArray_returnsEmptySet()
        {
            var set = SetUtil.ToSet(Array.Empty<string>());
            Assert.NotNull(set);
            Assert.Empty(set);
        }

        [Fact]
        public void ToSet_trimsAndSkipsNullAndBlank()
        {
            var set = SetUtil.ToSet(new[] { "  a  ", null, "", "  ", "b" });
            Assert.Equal(2, set.Count);
            Assert.Contains("a", set);
            Assert.Contains("b", set);
        }
    }
}
