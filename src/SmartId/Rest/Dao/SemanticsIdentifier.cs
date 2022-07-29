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

namespace SK.SmartId.Rest.Dao
{
    public struct SemanticsIdentifier : IEquatable<SemanticsIdentifier>
    {
        private readonly string identifier;

        public SemanticsIdentifier(IdentityType identityType, CountryCode countryCode, string identityNumber)
        {
            identifier = "" + identityType + countryCode + "-" + identityNumber;
        }

        public SemanticsIdentifier(IdentityType identityType, string countryCodeString, string identityNumber)
        {
            identifier = "" + identityType + countryCodeString + "-" + identityNumber;
        }

        public SemanticsIdentifier(string identityTypeString, string countryCodeString, string identityNumber)
        {
            identifier = "" + identityTypeString + countryCodeString + "-" + identityNumber;
        }

        public SemanticsIdentifier(string identifier)
        {
            this.identifier = identifier;
        }

        public string Identifier => identifier;

        public enum IdentityType
        {
            PAS, IDC, PNO
        }

        public enum CountryCode
        {
            EE, LT, LV
        }

        public override string ToString()
        {
            return "SemanticsIdentifier{" +
                "identifier='" + identifier + '\'' +
                '}';
        }

        public override bool Equals(object obj)
        {
            if (obj is SemanticsIdentifier other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(SemanticsIdentifier other)
        {
            return string.Equals(identifier, other.identifier, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            if (identifier == null)
            {
                return 0;
            }
            return identifier.GetHashCode();
        }

        public static bool operator ==(SemanticsIdentifier left, SemanticsIdentifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SemanticsIdentifier left, SemanticsIdentifier right)
        {
            return !(left == right);
        }
    }
}